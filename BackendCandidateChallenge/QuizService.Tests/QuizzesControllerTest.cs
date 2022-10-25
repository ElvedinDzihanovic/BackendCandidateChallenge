using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using QuizService.Controllers;
using QuizService.Model;
using QuizService.Model.Domain;
using Xunit;

namespace QuizService.Tests;

public class QuizzesControllerTest
{
    const string QuizApiEndPoint = "/api/quizzes/";

    [Fact]
    public async Task PostNewQuizAddsQuiz()
    {
        var quiz = new QuizCreateModel("Test title");
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(quiz));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),
                content);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }
    }

    [Fact]
    public async Task AQuizExistGetReturnsQuiz()
    {
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            const long quizId = 1;
            var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var quiz = JsonConvert.DeserializeObject<QuizResponseModel>(await response.Content.ReadAsStringAsync());
            Assert.Equal(quizId, quiz.Id);
            Assert.Equal("My first quiz", quiz.Title);
        }
    }

    [Fact]
    public async Task AQuizDoesNotExistGetFails()
    {
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            const long quizId = 999;
            var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
        
    public async Task AQuizDoesNotExists_WhenPostingAQuestion_ReturnsNotFound()
    {
        const string QuizApiEndPoint = "/api/quizzes/999/questions";

        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            var question = new QuestionCreateModel("The answer to everything is what?");
            var content = new StringContent(JsonConvert.SerializeObject(question));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),content);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    // NOTE: This test could have been done by calling each endpoint individually (create quiz, questions, answers etc.),
    // however I've decided to create a new CREATE test endpoint for creating the initial quiz
    // with questions and correct answers, just to make everything more efficient and readable
    [Fact]
    public async Task AQuizIsSuccessfulBasedOnNumberOfCorrectAnswers()
    {
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            var quiz = new QuizCreateModel("Test Quiz");
            var content = new StringContent(JsonConvert.SerializeObject(quiz));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"/api/quizzes/test"),
                content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Make sure quiz is created, along with questions and answers
            var getQuizResponse = await client.GetAsync(response.Headers.Location); // (new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"));

            var createdQuiz = JsonConvert.DeserializeObject<QuizResponseModel>(await getQuizResponse.Content.ReadAsStringAsync());

            Assert.NotNull(createdQuiz);

            // Get questions
            var firstQuestion = createdQuiz.Questions.FirstOrDefault();
            var secondQuestion = createdQuiz.Questions.LastOrDefault();


            // TODO: Comment this line...
            var checkAnswerEndpoint1 = $"/api/quizzes/{createdQuiz.Id}/questions/{firstQuestion.Id}/answers/{firstQuestion.Answers.FirstOrDefault()?.Id}/check";
            
            // ... and uncomment this one to make the quiz fail!
            // var checkAnswerEndpoint1 = $"/api/quizzes/{createdQuiz.Id}/questions/{firstQuestion.Id}/answers/{firstQuestion.Answers.LastOrDefault()?.Id}/check";


            var checkAnswerEndpoint2 = $"/api/quizzes/{createdQuiz.Id}/questions/{secondQuestion.Id}/answers/{secondQuestion.Answers.FirstOrDefault()?.Id}/check";


            var checkFirstAnswerResponse = await client.GetAsync(checkAnswerEndpoint1);
            var firstAnswerResult = JsonConvert.DeserializeObject<bool>(await checkFirstAnswerResponse.Content.ReadAsStringAsync());

            var checkSecondAnswerResponse = await client.GetAsync(checkAnswerEndpoint2);
            var secondAnswerResult = JsonConvert.DeserializeObject<bool>(await checkSecondAnswerResponse.Content.ReadAsStringAsync());

            // If both answers are correct, the quiz is passed!
            Assert.True(firstAnswerResult);
            Assert.True(secondAnswerResult);
        }
    }
}