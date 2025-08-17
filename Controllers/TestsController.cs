using System.Data;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel.Drawings;
using ExcelDataReader;                 // legacy .xls (no images)
using ClosedXML.Excel;                // NEW: .xlsx with images
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;    // NEW: for saving images

using quizTool.Models;

namespace quizTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly QuizTool_Dbcontext _db;
        private readonly IWebHostEnvironment _env; // NEW

        // CHANGED: inject IWebHostEnvironment to save images under wwwroot/uploads
        public TestsController(QuizTool_Dbcontext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ---------- Helpers (NEW) ----------
        private string EnsureUploadsDir()
        {
            var web = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var dir = Path.Combine(web, "uploads");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private string SaveImage(Stream src, string ext)
        {
            var dir = EnsureUploadsDir();
            var file = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(dir, file);
            using var fs = System.IO.File.Create(path);
            src.CopyTo(fs);
            return $"/uploads/{file}"; // public URL
        }

        // ---------- Admin upload (supports Type + images) ----------
        [Authorize(Roles = "admin")]
        [HttpPost("upload")]
        public async Task<ActionResult<UploadTestResultDto>> Upload([FromForm] IFormFile file, [FromForm] string? title)
        {
            if (file == null || file.Length == 0) return BadRequest("No file submitted.");

            var test = new Test
            {
                Title = string.IsNullOrWhiteSpace(title)
                    ? (Path.GetFileNameWithoutExtension(file.FileName) ?? "Uploaded Test")
                    : title.Trim()
            };

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Prefer .xlsx via ClosedXML to read embedded images and clean headers
            if (ext == ".xlsx")
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheet(1);

                // header map
                var header = ws.FirstRowUsed();
                var colMap = header.Cells().ToDictionary(
                    c => c.GetString().Trim(),
                    c => c.Address.ColumnNumber,
                    StringComparer.OrdinalIgnoreCase);

                int GetCol(string name, int fallback = -1)
                    => colMap.TryGetValue(name, out var n) ? n : fallback;

                int colType = GetCol("Type");
                int colQ = GetCol("Question");
                int colO1 = GetCol("Option 1", GetCol("Option1"));
                int colO2 = GetCol("Option 2", GetCol("Option2"));
                int colO3 = GetCol("Option 3", GetCol("Option3"));
                int colO4 = GetCol("Option 4", GetCol("Option4"));
                int colCorr = GetCol("Correct");
                int colModel = GetCol("ModelAnswer", GetCol("Answer"));

                if (colType < 0 || colQ < 0)
                    return BadRequest("Missing required columns: Type, Question");

                var pics = ws.Pictures.ToList(); // for images on Question cell
                int rowStart = header.RowBelow().RowNumber();
                int last = ws.LastRowUsed().RowNumber();

                for (int r = rowStart; r <= last; r++)
                {
                    string qText = ws.Cell(r, colQ).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(qText)) continue;

                    var typeStr = ws.Cell(r, colType).GetString().Trim().ToLowerInvariant();
                    var kind = typeStr == "subjective" ? QuestionType.Subjective : QuestionType.Objective;

                    // image anchored to question cell (top-left)
                    string? imageUrl = null;
                    var pic = pics.FirstOrDefault(p =>
                        p.TopLeftCell.Address.RowNumber == r &&
                        p.TopLeftCell.Address.ColumnNumber == colQ);

                    if (pic != null)
                    {
                        // Map enum to a file extension
                        var imgExt = pic.Format switch
                        {
                            XLPictureFormat.Png => ".png",
                            XLPictureFormat.Jpeg => ".jpg",
                            XLPictureFormat.Gif => ".gif",
                            XLPictureFormat.Bmp => ".bmp",
                            XLPictureFormat.Tiff => ".tiff",
                            _ => ".png"
                        };

                        using var ms = new MemoryStream();
                        // Ensure we copy the picture stream into our own stream
                        using (var src = pic.ImageStream)
                        {
                            if (src.CanSeek) src.Position = 0;
                            src.CopyTo(ms);
                        }
                        ms.Position = 0;

                        imageUrl = SaveImage(ms, imgExt); // saved to /uploads
                    }


                    var question = new Question
                    {
                        Text = qText,
                        Type = kind,
                        ImageUrl = imageUrl
                    };

                    if (kind == QuestionType.Objective)
                    {
                        var opts = new List<string>();
                        if (colO1 > 0) { var v = ws.Cell(r, colO1).GetString(); if (!string.IsNullOrWhiteSpace(v)) opts.Add(v.Trim()); }
                        if (colO2 > 0) { var v = ws.Cell(r, colO2).GetString(); if (!string.IsNullOrWhiteSpace(v)) opts.Add(v.Trim()); }
                        if (colO3 > 0) { var v = ws.Cell(r, colO3).GetString(); if (!string.IsNullOrWhiteSpace(v)) opts.Add(v.Trim()); }
                        if (colO4 > 0) { var v = ws.Cell(r, colO4).GetString(); if (!string.IsNullOrWhiteSpace(v)) opts.Add(v.Trim()); }

                        if (opts.Count == 0) continue;

                        int correctIndex = 0;
                        if (colCorr > 0)
                        {
                            var corrStr = ws.Cell(r, colCorr).GetString();
                            if (int.TryParse(corrStr, out var num) && num >= 1 && num <= opts.Count)
                                correctIndex = num - 1;
                        }

                        for (int i = 0; i < opts.Count; i++)
                            question.Options.Add(new Option { Text = opts[i], IsCorrect = i == correctIndex });
                    }
                    else
                    {
                        if (colModel > 0)
                        {
                            var modelAns = ws.Cell(r, colModel).GetString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(modelAns)) question.ModelAnswer = modelAns;
                        }
                    }

                    test.Questions.Add(question);
                }
            }
            else
            {
                // Legacy path (.xls) — no images; keep your previous logic
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using var stream = file.OpenReadStream();
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                });

                if (dataSet.Tables.Count == 0) return BadRequest("Empty Excel.");
                var table = dataSet.Tables[0];

                foreach (DataRow row in table.Rows)
                {
                    var typeStr = table.Columns.Contains("Type") ? row["Type"]?.ToString()?.Trim()?.ToLowerInvariant() : "objective";
                    var kind = typeStr == "subjective" ? QuestionType.Subjective : QuestionType.Objective;

                    string q = table.Columns.Contains("Question")
                        ? row["Question"]?.ToString()?.Trim() ?? ""
                        : row.ItemArray.Length > 1 ? row[1]?.ToString()?.Trim() ?? "" : "";

                    if (string.IsNullOrWhiteSpace(q)) continue;

                    var question = new Question { Text = q, Type = kind };

                    if (kind == QuestionType.Objective)
                    {
                        string? o1 = table.Columns.Contains("Option 1") ? row["Option 1"]?.ToString() :
                                     table.Columns.Contains("Option1") ? row["Option1"]?.ToString() : null;
                        string? o2 = table.Columns.Contains("Option 2") ? row["Option 2"]?.ToString() :
                                     table.Columns.Contains("Option2") ? row["Option2"]?.ToString() : null;
                        string? o3 = table.Columns.Contains("Option 3") ? row["Option 3"]?.ToString() :
                                     table.Columns.Contains("Option3") ? row["Option3"]?.ToString() : null;
                        string? o4 = table.Columns.Contains("Option 4") ? row["Option 4"]?.ToString() :
                                     table.Columns.Contains("Option4") ? row["Option4"]?.ToString() : null;

                        var opts = new[] { o1, o2, o3, o4 }
                                   .Where(s => !string.IsNullOrWhiteSpace(s))
                                   .Select(s => s!.Trim()).ToList();
                        if (opts.Count == 0) continue;

                        int correctIndex = 0;
                        if (table.Columns.Contains("Correct"))
                        {
                            var corr = row["Correct"]?.ToString()?.Trim();
                            if (int.TryParse(corr, out var num) && num >= 1 && num <= opts.Count) correctIndex = num - 1;
                        }

                        for (int i = 0; i < opts.Count; i++)
                            question.Options.Add(new Option { Text = opts[i], IsCorrect = i == correctIndex });
                    }
                    else
                    {
                        if (table.Columns.Contains("ModelAnswer"))
                            question.ModelAnswer = row["ModelAnswer"]?.ToString()?.Trim();
                        else if (table.Columns.Contains("Answer"))
                            question.ModelAnswer = row["Answer"]?.ToString()?.Trim();
                    }

                    test.Questions.Add(question);
                }
            }

            if (test.Questions.Count == 0)
                return BadRequest("No questions parsed. Check required columns: Type, Question (and Correct/options for objective, ModelAnswer for subjective).");

            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            return Ok(new UploadTestResultDto { TestId = test.Id, Title = test.Title, Questions = test.Questions.Count });
        }

        // ---------- List tests (auth required) ----------
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSummaryDto>>> GetTests()
        {
            var list = await _db.Tests
                .Select(t => new TestSummaryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    CreatedAt = t.CreatedAt,
                    QuestionCount = t.Questions.Count
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(list);
        }

        // ---------- Attempts (admin) ----------
        [Authorize(Roles = "admin")]
        [HttpGet("attempts")]
        public async Task<ActionResult<IEnumerable<AttemptListItemDto>>> GetAttempts([FromQuery] int? testId)
        {
            var query =
                from a in _db.TestAttempts
                join t in _db.Tests on a.TestId equals t.Id
                orderby a.AttemptedAt descending
                select new AttemptListItemDto
                {
                    Id = a.Id,
                    TestId = a.TestId,
                    TestTitle = t.Title,
                    UserEmail = a.UserEmail,
                    Score = a.Score,
                    Total = a.Total,
                    Percent = a.Total == 0 ? 0 : (int)Math.Round(100.0 * a.Score / a.Total),
                    AttemptedAt = a.AttemptedAt
                };

            if (testId.HasValue)
                query = query.Where(x => x.TestId == testId.Value)
                             .OrderByDescending(x => x.AttemptedAt);

            var list = await query.ToListAsync();
            return Ok(list);
        }

        // ---------- Get a single test (now includes Type + ImageUrl; never leaks IsCorrect) ----------
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TestDetailDto>> GetTest(int id)
        {
            var test = await _db.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null) return NotFound();

            return Ok(new TestDetailDto
            {
                Id = test.Id,
                Title = test.Title,
                Questions = test.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type == QuestionType.Subjective ? "subjective" : "objective", // NEW
                    ImageUrl = q.ImageUrl,                                                // NEW
                    Options = q.Type == QuestionType.Objective
                        ? q.Options.Select(o => new OptionDto { Id = o.Id, Text = o.Text }).ToList()
                        : new List<OptionDto>()
                }).ToList()
            });
        }

        // ---------- Delete test (admin) ----------
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == id);
            if (test == null) return NotFound(new { message = "Test not found." });

            _db.Tests.Remove(test);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Create test (admin) (NEW) ----------
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<TestSummaryDto>> CreateTest([FromBody] Test t)
        {
            if (string.IsNullOrWhiteSpace(t?.Title)) return BadRequest("Title required.");
            var test = new Test { Title = t.Title.Trim() };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            return Ok(new TestSummaryDto
            {
                Id = test.Id,
                Title = test.Title,
                CreatedAt = test.CreatedAt,
                QuestionCount = 0
            });
        }

        // ---------- Add question to existing test (admin) (NEW) ----------
        [Authorize(Roles = "admin")]
        [HttpPost("{testId:int}/questions")]
        public async Task<ActionResult> AddQuestion(
            int testId,
            [FromForm] string type,
            [FromForm] string text,
            [FromForm] string? modelAnswer,
            [FromForm] IFormFile? image,
            [FromForm] string[]? options,     // for objective
            [FromForm] int? correctIndex      // 0-based
        )
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == testId);
            if (test == null) return NotFound("Test not found.");

            var kind = (type ?? "").ToLowerInvariant() == "subjective" ? QuestionType.Subjective : QuestionType.Objective;

            string? imageUrl = null;
            if (image != null && image.Length > 0)
            {
                using var s = image.OpenReadStream();
                var e = Path.GetExtension(image.FileName);
                imageUrl = SaveImage(s, string.IsNullOrWhiteSpace(e) ? ".png" : e);
            }

            var q = new Question
            {
                TestId = testId,
                Text = text?.Trim() ?? "",
                Type = kind,
                ImageUrl = imageUrl
            };

            if (kind == QuestionType.Objective)
            {
                var opts = options?.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList() ?? new();
                if (opts.Count == 0) return BadRequest("Provide at least one option.");
                int ci = Math.Clamp(correctIndex ?? 0, 0, Math.Max(0, opts.Count - 1));
                for (int i = 0; i < opts.Count; i++)
                    q.Options.Add(new Option { Text = opts[i], IsCorrect = i == ci });
            }
            else
            {
                q.ModelAnswer = modelAnswer?.Trim();
            }

            _db.Questions.Add(q);
            await _db.SaveChangesAsync();
            return Ok(new { questionId = q.Id });
        }

        // ---------- Submit attempt (subjective stored, objective auto-graded) ----------
        [HttpPost("submit")]
        public async Task<ActionResult<AttemptResultDto>> Submit([FromBody] SubmitAttemptDto dto)
        {
            var test = await _db.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == dto.TestId);

            if (test == null) return NotFound("Test not found.");

            int totalObjective = test.Questions.Count(q => q.Type == QuestionType.Objective); // NEW
            int score = 0;

            var attempt = new TestAttempt
            {
                TestId = test.Id,
                UserEmail = string.IsNullOrWhiteSpace(dto.UserEmail) ? "anonymous@local" : dto.UserEmail
            };

            var correctByQuestion = test.Questions
                .Where(q => q.Type == QuestionType.Objective)
                .ToDictionary(
                    q => q.Id,
                    q => q.Options.FirstOrDefault(o => o.IsCorrect)?.Id
                );

            foreach (var ans in dto.Answers)
            {
                var q = test.Questions.FirstOrDefault(x => x.Id == ans.QuestionId);
                if (q == null) continue;

                if (q.Type == QuestionType.Objective)
                {
                    bool isCorrect = ans.SelectedOptionId.HasValue &&
                                     correctByQuestion.TryGetValue(ans.QuestionId, out var corrId) &&
                                     corrId == ans.SelectedOptionId.Value;

                    if (isCorrect) score++;

                    attempt.Answers.Add(new TestAttemptAnswer
                    {
                        QuestionId = ans.QuestionId,
                        SelectedOptionId = ans.SelectedOptionId,
                        IsCorrect = isCorrect,
                        SubjectiveText = null
                    });
                }
                else
                {
                    attempt.Answers.Add(new TestAttemptAnswer
                    {
                        QuestionId = ans.QuestionId,
                        SelectedOptionId = null,
                        IsCorrect = false,           // not auto-graded
                        SubjectiveText = ans.SubjectiveText
                    });
                }
            }

            attempt.Score = score;
            attempt.Total = totalObjective;

            _db.TestAttempts.Add(attempt);
            await _db.SaveChangesAsync();

            return Ok(new AttemptResultDto
            {
                AttemptId = attempt.Id,
                Score = score,
                Total = totalObjective
            });
        }

        // ---------- Legacy helpers kept for .xls path ----------
        private static string? TryGetByHeader(DataRow row, string[] headers)
        {
            foreach (var h in headers)
            {
                if (row.Table.Columns.Contains(h)) return row[h]?.ToString();
            }
            return null;
        }

        private static string? TryGetCell(DataRow row, int optIndexZeroToThree)
        {
            int idx = 2 + optIndexZeroToThree; // 0->2 (C), 1->3 (D), etc.
            if (row.ItemArray.Length > idx) return row[idx]?.ToString();
            return null;
        }
    }
}
