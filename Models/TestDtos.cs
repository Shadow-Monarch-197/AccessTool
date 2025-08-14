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
    }

    public class AttemptResultDto
    {
        public int AttemptId { get; set; }
        public int Score { get; set; }
        public int Total { get; set; }
    }
}