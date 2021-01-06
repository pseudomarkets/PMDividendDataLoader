# PMDividendDataLoader
A .NET Core application designed to be run using Task Scheduler or cron that loads dividend data for stocks and ETFs held in user accounts.

# Requirements
* .NET Core 3.1
* Pseudo Markets instance with the latest SQL Server database schema
* MongoDB 4.x configured for Pseudo Markets non-relational storage (additional documentation available soon)

# Usage
The PMDividendDataLoader is meant to be run as an SOD proccess that can be scheduled through Task Scheduler on Windows or cron on Linux. Due to rate limiting through AlphaVantage's APIs, this service is recommended to be run once a week during the weekend. The PMDividendExecutionService (WIP) will take care of applying dividends as part of a daily process by reading the "cached" dividend data loaded into Mongo DB by this service.
