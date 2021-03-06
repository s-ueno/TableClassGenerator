# TableClassGenerator


It is a tool to generate a model that uses the "Dapper.Aggrigator".

This is a tool for developers to use.


Developers you must first adjust the connectionStrings section of the app.config file to your environment after you download this.

For example, if you connect to SQL-Server, type the such as the server name in the app.config to build, as shown below.

![PIC_1](http://s-ueno.github.io/images/TableClassGenerator_ConnectionString.PNG)

Please refer to [here](https://msdn.microsoft.com/library/ms254500.aspx) for the connectionStrings.

Introduce the details of the provider-specific connection string.

 + [SQL Server](https://msdn.microsoft.com/library/system.data.sqlclient.sqlconnection.connectionstring.aspx)
 + [OleDb](https://msdn.microsoft.com/library/system.data.oledb.oledbconnection.connectionstring.aspx)
 + [Odbc](https://msdn.microsoft.com/library/system.data.odbc.odbcconnection.connectionstring.aspx)
 + [OracleClient](https://msdn.microsoft.com/library/system.data.oracleclient.aspx)

can connect to any data source by extending the provider.  
[Npgsql](http://www.npgsql.org/) is famous as a library to be connected to the "PostgreSQL".  
These can be readily utilized by utilizing the nuget package.    

sample, will be in the configuration file using the Npgsql.  

![PIC_2](http://s-ueno.github.io/images/TableClassGenerator_PostgresProvider.PNG)








