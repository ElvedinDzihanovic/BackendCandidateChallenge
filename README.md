# Backend Candidate Challenge

This is a simple challenge to be used as part of an interview process for .NET backend developers. The challenge is intended to be done offline at your own speed using the tools of your choice. There is no single correct solution, just a set of tasks to trigger some relevant discussions. Be prepared to present your solution when we meet.   

#### Make a copy of this repo. Yes, we know that "Fork" exists, but please don't use it.

## Some notes on this task
1. For existing tests, I just made necessary changes and modifications to make them run correctly
2. For challenge task number 4. ("Create new test where you create a quiz with minimum two questions...") I made some custom endpoints and left detailed comments
3. I made some changes to architecture and added some additional improvements (swagger documentation and docker file)

## Running docker
1. docker build -t quizservice .
2. docker run -d -p 8080:80 --name quizcontainer quizservice
3. Open the app in localhost:8080/swagger