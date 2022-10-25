using QuizService.Model;
using QuizService.Model.Domain;
using System.Collections;
using System.Collections.Generic;

namespace QuizService.Repositories.Interfaces
{
    public interface IQuizRepository
    {
        public IEnumerable<QuizResponseModel> Get();
        public QuizResponseModel Get(int id);
        public QuizResponseModel.QuestionItem GetQuestion(int id);
        public object Create(QuizCreateModel quiz);
        public object CreateTestQuiz(QuizCreateModel quiz);
        public bool CheckAnswer(int questionId, int answerId);
    }
}
