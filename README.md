# SOFTENG 761 - Scrumpies (Back end repo)

## Project Description
This is the back-end API for Hitch Building Inspections Report Automation. The back-end manages the database (SQL), PDF generation, user management and automated email services.

## Build Status
Master branch: [![Build Status](https://travis-ci.com/Karim-C/Inspection_Report_App_back-end.svg?token=aUW8TwnwNhqKCHbwaCXT&branch=master)](https://travis-ci.com/Karim-C/Inspection_Report_App_back-end)

## Quick start
1. Clone and set up the Visual Studio project
2. Open and run the solution. ([Mac Instructions](https://github.com/KerwinSun/Inspection_Report_App_front-end/wiki/Using-Local-SQL-Server-on-Mac))
3. Use [Postman](https://www.getpostman.com/) to send the REST API calls.

## Prerequisites
* [.NET Core 2.1 Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-2.1)
* [Azure](https://azure.microsoft.com/en-us/)

Back-end is set up using .NET Core 2.1 Web API

The SQL Database is published on Azure.

Continuous Deployment is configured on Azure and any code merged into master is deployed.

## Link to Frontend

https://github.com/KerwinSun/Inspection_Report_App_front-end

## Link to the App

https://inspection-report-app.azurewebsites.net


## Link to the API
Here is the link to the API:

https://inspectionreportservice.azurewebsites.net/api/

## An example of a URI
An example of a URI for a GET request to retrieve all Todo Items in the database (dummy API controller):

https://inspectionreportservice.azurewebsites.net/api/todo
