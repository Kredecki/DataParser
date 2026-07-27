How to run:
1. Use command: `dotnet run --project .\DataParser\DataParser.API.csproj` in your terminal
2. Open your browser and enter site: https://localhost:7001/swagger/index.html

DataParser is a simple parser that uses the CQRS architectural pattern. I implemented simple authorization in it. We don't have a connected database, so I skipped the login and password check. Any login and password will be valid. For example:
`{
  "login": "string",
  "password": "string"
}`
would be good. Response should look like this:
`{
  "login": "string",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9zaWQiOiJkNmYyZmUxNS02NGFjLTRiYTctYWQzZi1jNjQxMzI3NzJiZDQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3RyaW5nIiwiZXhwIjoxNzg1MTg5NTk5LCJpc3MiOiJEYXRhUGFyc2VyIiwiYXVkIjoiRGF0YVBhcnNlciJ9.iV-3cndnVpSEVD3rfJStichaADpiO7nkoFVr5HOFlCc"
}`
We need to copy the token and paste it into Swagger to authorize access to the parse-content endpoint.

DataParser is build with strategy pattern, so we can extend it with more new parsers.
