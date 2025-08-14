using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;


using System.Data;
using System.Text;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quizTool.Models;

namespace quizTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly QuizTool_Dbcontext _db;
        public TestsController(QuizTool_Dbcontext db) => _db = db;

        
        
        // Admin upload
        [Authorize(Roles = "admin")]
        [HttpPost("upload")]
        public async Task<ActionResult<UploadTestResultDto>> Upload([FromForm] IFormFile file, [FromForm] string? title)
        {
            if (file == null || file.Length == 0) return BadRequest("No file submitted.");

            // Register encoding provider for .xls
            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            if (dataSet.Tables.Count == 0) return BadRequest("Empty Excel.");

            var table = dataSet.Tables[0];

            var test = new Test
            {
                Title = string.IsNullOrWhiteSpace(title) ? (Path.GetFileNameWithoutExtension(file.FileName) ?? "Uploaded Test") : title.Trim()
            };

            foreach (DataRow row in table.Rows)
            {
                string q = row.Table.Columns.Contains("Question")
                ? row["Question"]?.ToString()?.Trim() ?? ""
                : row.ItemArray.Length > 1 ? row[1]?.ToString()?.Trim() ?? "" : "";

                if (string.IsNullOrWhiteSpace(q)) continue;

                var question = new Question { Text = q };
                var options = new List<(string text, bool isCorrect)>();

                // Option columns (try common names, else fallback C..F)
                string[] optCols = new[] { "Option1", "Option 1", "opt1", "C",
                 "Option2", "Option 2", "opt2", "D",
                 "Option3", "Option 3", "opt3", "E",
                 "Option4", "Option 4", "opt4", "F" };

                var picked = new List<string>();
                for (int i = 0; i < 4; i++)
                {
                    string txt = TryGetCell(row, i) ?? "";
                    picked.Add(txt);
                }

                // If we had headers, build from named columns instead:
                string? o1 = TryGetByHeader(row, new[] { "Option1", "Option 1", "opt1" }) ?? picked.ElementAtOrDefault(0);
                string? o2 = TryGetByHeader(row, new[] { "Option2", "Option 2", "opt2" }) ?? picked.ElementAtOrDefault(1);
                string? o3 = TryGetByHeader(row, new[] { "Option3", "Option 3", "opt3" }) ?? picked.ElementAtOrDefault(2);
                string? o4 = TryGetByHeader(row, new[] { "Option4", "Option 4", "opt4" }) ?? picked.ElementAtOrDefault(3);

                var opts = new[] { o1, o2, o3, o4 }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                if (opts.Count == 0) continue;

                // Detect correct using "(correct option)" marker OR Correct column
                int? correctIndex = null;

                // Marker in text
                for (int i = 0; i < opts.Count; i++)
                {
                    if (opts[i].Contains("(correct option)", StringComparison.OrdinalIgnoreCase))
                    {
                        opts[i] = opts[i].Replace("(correct option)", "", StringComparison.OrdinalIgnoreCase).Trim();
                        correctIndex = i;
                        break;
                    }
                }

                // "Correct" column with 1..4
                if (!correctIndex.HasValue && row.Table.Columns.Contains("Correct"))
                {
                    var corr = row["Correct"]?.ToString()?.Trim();
                    if (int.TryParse(corr, out var num) && num >= 1 && num <= opts.Count) correctIndex = num - 1;
                }

                // Default: first one if still missing (avoid orphan questions)
                correctIndex ??= 0;

                for (int i = 0; i < opts.Count; i++)
                {
                    question.Options.Add(new Option
                    {
                        Text = opts[i].Trim(),
                        IsCorrect = i == correctIndex.Value
                    });
                }

                test.Questions.Add(question);
            }

            if (test.Questions.Count == 0) return BadRequest("No questions parsed. Check column names and markers.");

            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            return Ok(new UploadTestResultDto { TestId = test.Id, Title = test.Title, Questions = test.Questions.Count });
        }

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

        //new code for attempts
        [Authorize(Roles = "admin")]
        [HttpGet("attempts")]
        public async Task<ActionResult<IEnumerable<AttemptListItemDto>>> GetAttempts([FromQuery] int? testId)
        {
            // Join to get Test title without assuming navigation properties
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
        // Get a single test (no correct flags)
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
                    Options = q.Options.Select(o => new OptionDto { Id = o.Id, Text = o.Text }).ToList()
                }).ToList()
            });
        }

        //newly added
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

        // Submit attempt
        [HttpPost("submit")]
        public async Task<ActionResult<AttemptResultDto>> Submit([FromBody] SubmitAttemptDto dto)
        {
            var test = await _db.Tests
            .Include(t => t.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(t => t.Id == dto.TestId);

            if (test == null) return NotFound("Test not found.");

            int score = 0;
            int total = test.Questions.Count;

            var attempt = new TestAttempt
            {
                TestId = test.Id,
                UserEmail = string.IsNullOrWhiteSpace(dto.UserEmail) ? "anonymous@local" : dto.UserEmail
            };

            // Build a quick lookup for correctness
            var correctByQuestion = test.Questions.ToDictionary(
                 q => q.Id,
                 q => q.Options.FirstOrDefault(o => o.IsCorrect)?.Id
                 );

            foreach (var ans in dto.Answers)
            {
                bool isCorrect = ans.SelectedOptionId.HasValue &&
                correctByQuestion.TryGetValue(ans.QuestionId, out var corrId) &&
                corrId == ans.SelectedOptionId.Value;

                if (isCorrect) score++;

                attempt.Answers.Add(new TestAttemptAnswer
                {
                    QuestionId = ans.QuestionId,
                    SelectedOptionId = ans.SelectedOptionId,
                    IsCorrect = isCorrect
                });
            }

            attempt.Score = score;
            attempt.Total = total;

            _db.TestAttempts.Add(attempt);
            await _db.SaveChangesAsync();

            return Ok(new AttemptResultDto
            {
                AttemptId = attempt.Id,
                Score = score,
                Total = total
            });
        }
    }
}