## Some notes on this task
1. For existing tests, I just made necessary changes and modifications to make them run correctly
2. For challenge task number 4. ("Create new test where you create a quiz with minimum two questions...") I made some custom endpoints and left detailed comments
3. I made some changes to architecture and added some additional improvements (swagger documentation and docker file)

## Running docker
1. In BackendCandidateChallenge\QuizService folder
2. docker build -t quizservice .
3. docker run -d -p 8080:80 --name quizcontainer quizservice
4. Open the app in localhost:8080/swagger