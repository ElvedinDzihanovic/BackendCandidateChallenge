using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using QuizService.Model.Domain;
using System.Linq;
using QuizService.Repositories.Interfaces;
using System;

namespace QuizService.Controllers;

[Route("api/quizzes")]
public class QuizController : Controller
{
    private readonly IDbConnection _connection;
    private readonly IQuizRepository _quizRepository;

    /* TODO: In general, we could use the classic "repository - service - controller" approach here
     * but for simplicity I've decided to create just a simple QuizRepository and move some of the logic there. Of course, we shouldn't have
     * db logic directly in controller, as Services/Repositories should be in separate class libraries.
     * 
     * Additionally, I'd prefer to separate quiz logic from questions and answers (have separate controllers,
     * services etc.).
     * 
    */
    public QuizController(IDbConnection connection, IQuizRepository quizRepository)
    {
        _connection = connection;
        _quizRepository = quizRepository;
    }

    // GET api/quizzes
    [HttpGet]
    public ActionResult<IEnumerable<QuizResponseModel>> Get()
    {
        var result = _quizRepository.Get();
        return Ok(result);
    }

    // GET api/quizzes/5
    [HttpGet("{id}")]
    public ActionResult<object> Get(int id)
    {
        var result = _quizRepository.Get(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // POST api/quizzes
    [HttpPost]
    public IActionResult Post([FromBody]QuizCreateModel value)
    {
        var id = _quizRepository.Create(value);
        return Created($"/api/quizzes/{id}", null);
    }

    // NOTE: Custom endpoint for quickly creating test quiz with questions and answers
    // POST api/quizzes/test
    [HttpPost("test")]
    public IActionResult PostTestQuiz([FromBody] QuizCreateModel value)
    {
        var id = _quizRepository.CreateTestQuiz(value);
        return Created($"/api/quizzes/{id}", null);
    }

    // PUT api/quizzes/5
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody]QuizUpdateModel value)
    {
        const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
        int rowsUpdated = _connection.Execute(sql, new {Id = id, Title = value.Title});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        const string sql = "DELETE FROM Quiz WHERE Id = @Id";
        int rowsDeleted = _connection.Execute(sql, new {Id = id});
        if (rowsDeleted == 0)
            return NotFound();
        return NoContent();
    }

    // GET api/quizzes/5
    [HttpGet("{id}/questions/{qid}")]
    public ActionResult<QuizResponseModel.QuestionItem> GetQuestion(int id, int qid)
    {
        var result = _quizRepository.GetQuestion(qid);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // POST api/quizzes/5/questions
    [HttpPost]
    [Route("{id}/questions")]
    public IActionResult PostQuestion(int id, [FromBody]QuestionCreateModel value)
    {
        const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";

        var quiz = _quizRepository.Get(id);

        if (quiz == null)
            return NotFound();
        
        var questionId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuizId = id});
        return Created($"/api/quizzes/{id}/questions/{questionId}", null);
    }

    // PUT api/quizzes/5/questions/6
    [HttpPut("{id}/questions/{qid}")]
    public IActionResult PutQuestion(int id, int qid, [FromBody]QuestionUpdateModel value)
    {
        const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
        int rowsUpdated = _connection.Execute(sql, new {QuestionId = qid, Text = value.Text, CorrectAnswerId = value.CorrectAnswerId});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6
    [HttpDelete]
    [Route("{id}/questions/{qid}")]
    public IActionResult DeleteQuestion(int id, int qid)
    {
        const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
        _connection.ExecuteScalar(sql, new {QuestionId = qid});
        return NoContent();
    }

    // NOTE: This endpoint checks if selected answer is correct for the given question and returns number of scored points
    [HttpGet]
    [Route("{id}/questions/{qid}/answers/{aid}/check")]
    public ActionResult<int> CheckAnswer(int id, int qid, int aid)
    {
        var result = _quizRepository.CheckAnswer(qid, aid);

        // If the answer is correct, player scores 1 point
        var scoredPoints = result ? 1 : 0;

        return Ok(scoredPoints);
    }

    // POST api/quizzes/5/questions/6/answers
    [HttpPost]
    [Route("{id}/questions/{qid}/answers")]
    public IActionResult PostAnswer(int id, int qid, [FromBody]AnswerCreateModel value)
    {
        const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
        var answerId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuestionId = qid});
        return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", null);
    }

    // PUT api/quizzes/5/questions/6/answers/7
    [HttpPut("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult PutAnswer(int id, int qid, int aid, [FromBody]AnswerUpdateModel value)
    {
        const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
        int rowsUpdated = _connection.Execute(sql, new {AnswerId = qid, Text = value.Text});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6/answers/7
    [HttpDelete]
    [Route("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult DeleteAnswer(int id, int qid, int aid)
    {
        const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
        _connection.ExecuteScalar(sql, new {AnswerId = aid});
        return NoContent();
    }
}