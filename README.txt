Name: Bharat Murali

Opscode Starter Project

Details:

1) I am using files to mimic a database. All my lookups up are linear - while they would be much faster (O (log N)) if I were using a db with SQL queries. I hope you can excuse that. To make this faster, I could store by data in an in-memory hashtable for O(1) access.

2) Like I said, the file structure I have chosen is intended to mimic the way I would have set up a database. This means I would have a Users table, a Groups table and another table to map both of them to each other.

3) I was debating between unit tests and overall integration tests. I ended up going with integration tests so that you can see the entire end-to-end flow of the API.

4) I do not have a web UI that you can interact with. Right now, the extent of the API is self-contained to the tests and injecting calls through Fiddler or some other HttpClient.

5) Since the service leverages disk for storage, you may need to run it as administrator so that it has permissions to store on disk.

6) API is developed as a C# MVC Rest API

How to run:

OS: Windows

This folder contains 2 VS solutions:

1) Open the OpcodeStarter solution first (sorry for the typo!)
2) After it has loaded, you should be able to build and run locally by hitting F5 or by hitting Debug -> Begin Debugging.
3) This will open an IE instance and will default to a 404 page (Like I said, I did not build any views)
4) Now the the service is running!

5) To run tests, open the other solution (OpscodeStarterTests).
6) Once is has loaded, Click on Test -> Run -> All Tests
7) My tests are hardcoded to hit http://localhost:60081, which should be where the service is hosted. If it is not, please update it according to the port number in the IE address bar.
8) All the tests should pass.

Final note: The process is quite manual right now and I appreciate your patience for bearing with it. Please let me know if you have any trouble with the process.