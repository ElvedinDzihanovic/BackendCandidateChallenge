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

    }
}
