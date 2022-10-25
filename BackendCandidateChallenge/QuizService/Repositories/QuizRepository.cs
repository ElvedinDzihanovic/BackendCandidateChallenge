using Dapper;
using QuizService.Model;
using QuizService.Model.Domain;
using QuizService.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;

namespace QuizService.Repositories
{
    // TODO: Refactor everything
    public class QuizRepository : IQuizRepository
    {
        private readonly IDbConnection _connection;

        public QuizRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public IEnumerable<QuizResponseModel> Get()
        {
            const string sql = "SELECT * FROM Quiz;";
            var quizzes = _connection.Query<Quiz>(sql);
            return quizzes.Select(quiz =>
                new QuizResponseModel
                {
                    Id = quiz.Id,
                    Title = quiz.Title
                });
        }

        public QuizResponseModel Get(int id)
        {
            const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
            var quiz = _connection.QueryFirstOrDefault<Quiz>(quizSql, new { Id = id });
            
            if (quiz == null)
                return null;

            const string questionsSql = "SELECT * FROM Question WHERE QuizId = @QuizId;";
            var questions = _connection.Query<Question>(questionsSql, new { QuizId = id });
            const string answersSql = "SELECT a.Id, a.Text, a.QuestionId FROM Answer a INNER JOIN Question q ON a.QuestionId = q.Id WHERE q.QuizId = @QuizId;";
            var answers = _connection.Query<Answer>(answersSql, new { QuizId = id })
                .Aggregate(new Dictionary<int, IList<Answer>>(), (dict, answer) => {
                    if (!dict.ContainsKey(answer.QuestionId))
                        dict.Add(answer.QuestionId, new List<Answer>());
                    dict[answer.QuestionId].Add(answer);
                    return dict;
                });
            return new QuizResponseModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Questions = questions.Select(question => new QuizResponseModel.QuestionItem
                {
                    Id = question.Id,
                    Text = question.Text,
                    Answers = answers.ContainsKey(question.Id)
                        ? answers[question.Id].Select(answer => new QuizResponseModel.AnswerItem
                        {
                            Id = answer.Id,
                            Text = answer.Text
                        })
                        : new QuizResponseModel.AnswerItem[0],
                    CorrectAnswerId = question.CorrectAnswerId
                }),
                Links = new Dictionary<string, string>
            {
                {"self", $"/api/quizzes/{id}"},
                {"questions", $"/api/quizzes/{id}/questions"}
            }
            };
        }

        public QuizResponseModel.QuestionItem GetQuestion(int id)
        {
            const string questionSql = "SELECT * FROM Question WHERE Id = @Id;";
            var question = _connection.QueryFirstOrDefault<Question>(questionSql, new { Id = id });

            return question == null ? null : new QuizResponseModel.QuestionItem
            {
                Id = question.Id,
                CorrectAnswerId = question.CorrectAnswerId,
                Text = question.Text
            };
        }

        public object Create(QuizCreateModel quiz)
        {
            var sql = $"INSERT INTO Quiz (Title) VALUES('{quiz.Title}'); SELECT LAST_INSERT_ROWID();";
            var id = _connection.ExecuteScalar(sql);
            return id;
        }


        // Creating test quiz with questions and correct answers
        public object CreateTestQuiz(QuizCreateModel quiz)
        {
            // Create quiz
            var quizSql = $"INSERT INTO Quiz (Title) VALUES('{quiz.Title}'); SELECT LAST_INSERT_ROWID();";
            var quizId = _connection.ExecuteScalar(quizSql);

            // create questions
            const string questionsSql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";

            var firstQuestionId = _connection.ExecuteScalar(questionsSql, new { Text = "First Question Text", QuizId = quizId });
            var secondQuestionId = _connection.ExecuteScalar(questionsSql, new { Text = "Second Question Text", QuizId = quizId });

            // add answers

            const string answersSql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
            var firstAnswerId = _connection.ExecuteScalar(answersSql, new { Text = "Answer 1", QuestionId = firstQuestionId });
            var secondAnswerId = _connection.ExecuteScalar(answersSql, new { Text = "Answer 2", QuestionId = firstQuestionId });

            var thirdAnswerId = _connection.ExecuteScalar(answersSql, new { Text = "Answer 3", QuestionId = secondQuestionId });
            var fourthAnswerId = _connection.ExecuteScalar(answersSql, new { Text = "Answer 4", QuestionId = secondQuestionId });


            // add correct answers
            const string correctAnswersSql = "UPDATE Question SET CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
            int answerFirstQuestion = _connection.Execute(correctAnswersSql, new { QuestionId = firstQuestionId, CorrectAnswerId = firstAnswerId });
            int answerSecondQuestion = _connection.Execute(correctAnswersSql, new { QuestionId = secondQuestionId, CorrectAnswerId = thirdAnswerId });

            return quizId;
        }

        public bool CheckAnswer(int questionId, int answerId)
        {
            const string questionSql = "SELECT * FROM Question WHERE Id = @Id;";
            var question = _connection.QueryFirstOrDefault<Question>(questionSql, new { Id = questionId });

            return question?.CorrectAnswerId == answerId;
        }

    }


}
