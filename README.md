# SubSonic-Core
The driving force behind this project is the various issues with the current EF DAL available for .net core 2.2 during 2019.
various issues in which prevented minimal manipulation of the data to release projects.
This way I know the DAL from the ground up and speak for its performance and implemenation. 

# Project Goals
1. minimize the references to 3rd party projects which use namespaces that can come into conflict with the .net core library.
   1. that being said extensions are easy to override, just remember to forward the call if it does not apply.
2. expand understanding of the expression tree implemenation, expressions really are a rosetta stone for developers.
3. keep the SQL statements generated by the DAL as simple as possible.
4. utilize MARS statements when eagerloading data for an object graph.
5. implement asynchonous operations at some point.
6. support SQL Server, Oracle, MySql, etc. Database engines.
7. apply TDD, DRY, YAGNI, and KISS priciples were applicable.


# Reference Material
https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.assemblybuilder
https://www.databasejournal.com/features/mssql/article.php/3766391/SqlCredit-Part-18-Exploring-the-Performance-of-SQL-2005146s-OUTPUT-Clause.htm

# Reference Projects
This Project has been influenced by previous iterations of SubSonic up to and including Entity Frameworks Feature Set.
