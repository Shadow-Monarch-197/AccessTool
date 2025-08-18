namespace quizTool.Models
{
    public class UploadTestResultDto
    {
        public int TestId { get; set; }
        public string Title { get; set; }
        public int Questions { get; set; }
    }

    public class TestSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int QuestionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TestDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }

        // NEW: "objective" or "subjective"
        public string Type { get; set; } = "objective";

        // NEW: optional question image (served from /uploads/…)
        public string? ImageUrl { get; set; }

        public List<OptionDto> Options { get; set; }
    }

    public class OptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        // No IsCorrect here to avoid leaking answers
    }

    public class SubmitAttemptDto
    {
        public int TestId { get; set; }
        public string UserEmail { get; set; }
        public List<AnswerDto> Answers { get; set; }
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }

        // NEW: For subjective questions (free-text response)
        public string? SubjectiveText { get; set; }
    }

    public class AttemptResultDto
    {
        public int AttemptId { get; set; }
        public int Score { get; set; }
        public int Total { get; set; }
    }
    //// ====== Admin: Attempts list ======

    //// NEW: used by GET /api/Tests/attempts
    public class AttemptListItemDto
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string TestTitle { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public int Score { get; set; }
        public int Total { get; set; }
        public int Percent { get; set; }
        public DateTime AttemptedAt { get; set; }
    }

    //// ====== Admin: Attempt detail (to review subjective answers) ======

    //// NEW: shape for a single attempt with all answers expanded
    //public class AttemptDetailDto
    //{
    //    public int AttemptId { get; set; }
    //    public int TestId { get; set; }
    //    public string TestTitle { get; set; } = "";
    //    public string UserEmail { get; set; } = "";
    //    public int Score { get; set; }
    //    public int Total { get; set; }
    //    public DateTime AttemptedAt { get; set; }
    //    public List<AttemptAnswerDetailDto> Answers { get; set; } = new();
    //}

    //// NEW: each answer row in AttemptDetailDto
    //public class AttemptAnswerDetailDto
    //{
    //    public int QuestionId { get; set; }
    //    public string QuestionText { get; set; } = "";
    //    public string Type { get; set; } = "objective";   // "objective" | "subjective"
    //    public string? ImageUrl { get; set; }

    //    // Objective
    //    public int? SelectedOptionId { get; set; }
    //    public string? SelectedOptionText { get; set; }
    //    public bool? IsCorrect { get; set; }
    //    public string? CorrectOptionText { get; set; }     // helpful for admin review

    //    // Subjective
    //    public string? SubjectiveText { get; set; }
    //    public string? ModelAnswer { get; set; }           // optional reference for admin
    //}

    //// ====== Admin: Update score after review ======

    //// NEW: used by POST /api/Tests/attempts/{attemptId}/score
    //public class UpdateAttemptScoreDto
    //{
    //    public int AttemptId { get; set; }
    //    public int NewScore { get; set; }
    //}

    //// ====== Admin: View a test with answers (including correct flags) ======

    //// NEW: used by GET /api/Tests/{id}/admin-view
    //public class AdminTestViewDto
    //{
    //    public int Id { get; set; }
    //    public string Title { get; set; } = "";
    //    public List<AdminQuestionViewDto> Questions { get; set; } = new();
    //}

    //// NEW
    //public class AdminQuestionViewDto
    //{
    //    public int Id { get; set; }
    //    public string Text { get; set; } = "";
    //    public string Type { get; set; } = "objective";
    //    public string? ImageUrl { get; set; }
    //    public string? ModelAnswer { get; set; }
    //    public List<AdminOptionViewDto> Options { get; set; } = new();
    //}

    //// NEW
    //public class AdminOptionViewDto
    //{
    //    public int Id { get; set; }
    //    public string Text { get; set; } = "";
    //    public bool IsCorrect { get; set; }
    //}

    public class AdminTestDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<AdminQuestionDto> Questions { get; set; } = new();
    }

    public class AdminQuestionDto
    {
        public int Id { get; set; }
        public QuestionType Type { get; set; }
        public string Text { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? ModelAnswer { get; set; }
        public List<AdminOptionDto> Options { get; set; } = new();
    }

    public class AdminOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public bool IsCorrect { get; set; }
    }

    // --- NEW: Attempt details for review ---
    public class AttemptDetailDto
    {
        public int AttemptId { get; set; }
        public int TestId { get; set; }
        public string TestTitle { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public int Score { get; set; }
        public int Total { get; set; }
        public DateTime AttemptedAt { get; set; }
        public List<AttemptAnswerDetailDto> Answers { get; set; } = new();
    }

    public class AttemptAnswerDetailDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = "";

        // CHANGED: was QuestionType, now string ('objective' | 'subjective')
        public string Type { get; set; } = "objective";   // CHANGED

        public string? ImageUrl { get; set; }

        // Objective
        public int? SelectedOptionId { get; set; }
        public string? SelectedOptionText { get; set; }
        public int? CorrectOptionId { get; set; }
        public string? CorrectOptionText { get; set; }
        public bool? IsCorrect { get; set; }     // null for subjective

        // Subjective
        public string? SubjectiveText { get; set; }       // student's free-text
        public string? ModelAnswer { get; set; }          // optional reference
    }
    // --- NEW: score patch body ---
    public class UpdateAttemptScoreDto
    {
        public int Score { get; set; }
    }
}