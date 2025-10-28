# Fantasy Football Assistant Manager Backend

### Contributors: Eric, Noah, Temi, Zac

## Table of Contents

- [Project Overview](#project-overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Setup & Installation](#setup--installation)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)

## Project Overview

This is the research project demo version of the backend for our fantasy football assistant manager app. The repo for the frontend can be found here:
https://github.com/EricNohara/Fantasy-Football-Assistant-Manager-Frontend  
The purpose of this research project was to explore the tech stack and API options available to us and test out implementing our selections. More specifically, we have researched databases, AI APIs, NFL statistic APIs, and payment APIs. We selected and tested Supabase, OpenAI API, NFLVerse API, and Stripe API. The version of our app on this test branch contains basic functionality tests for each of these four components.

## Features

- Swagger UI for testing APIs
- Card payment frontend UI for mock Stripe payments
- Supabase database table querying
- ChatGPT querying
- Retrieving data from NFLVerse
- Making mock payments to Stripe

## Tech Stack

- **Backend Framework:** ASP.NET Core Web API (.NET 8)
- **Database:** Supabase (PostgreSQL)
- **Authentication:** Supabase JWT Authentication
- **Language:** C#
- **Version Control:** Git

## Setup & Installation

The backed and frontend code can be downloaded from this repository and  
https://github.com/EricNohara/Fantasy-Football-Assistant-Manager-Frontend  
respectively. On downloading, navigate to each solution directory from the command line and enter "dotnet restore" to install all necessary packages. Obtain your own API keys and accounts for Supabase, NFLVerse, OpenAI, and Stripe. Create the file "appsettings.json" with body:  
```
{
  //supabase keys
  "Supabase": {
    "Url": ,
    "ServiceRoleKey": 
  },
  //stripe keys
  "Stripe": {
    "PublishableKey": ,
    "SecretKey": ,
    "WebhookSecret": 
  },
  "OpenAI": {
    "ApiKey": 
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  //Indicates hosts from which requests can be accepted. Currently set to accept all.
  "AllowedHosts": "*"
}
```
Fill in all empty fields with your API keys. 
In order to test Stripe webhooks, you will have to download the Stripe CLI. More information can be found here:
https://docs.stripe.com/stripe-cli  
After downloading, open the command line and run "stripe login". Follow the instructions that appear to link this machine to your Stripe account.

## Api Endpoints

List endpoints here

## Testing
Most features of this test implementation can be tested without the need for the frontend by using Swagger. Open the command line, navigate to the solution directory, and enter "dotnet run --launch-profile "https". Copy the local URL that is displayed into a browser, adding "/swagger" to the end. This will bring you to a simple testing interface.  
# Supabase Testing
Select the Supabase endpoint and click "try it out". Enter into the table field the name of any table from the project's database scheme (such as "Users") and press "execute". This will retrieve all entries for the given table from the project database. Note that this database is not yet fully populated.  
# ChatGPT Testing
Select the ChatGPT test endpoint and click "try it out". Enter into the textbox a quotation-enclosed string and press "execute". You will be shown a response to your string from ChatGPT. 
# NFLVerse Testing
Select the Players POST endpoint, click "try it out", then click "execute". This will retrieve all NFL player data from NFLVerse and store it in the project Supabase database. You can view these results using the Supabase testing instructions and the table name "Players".
# Stripe Testing
In order to see the logs for Stripe events, stop the backend and enter "set ASPNETCORE_ENVIRONMENT=Development" so that debug logs will appear on the console. Stripe testing requires the frontend to be running in addition to the backend, so set up and run the frontend as instructed in its README. Open the command line and enter: 
"stripe listen --forward-to https://localhost:<local port>/api/stripewebhook/check-payment-completion"  
This establishes your machine as a temporary webhook endpoint. From the frontend UI, enter the following card info:  
- number: 4111 1111 1111 1111
- expiration date: any future month/year
- CVC: 111
This is a fake credit card used for testing. Press "Pay $10" to initiate payment. On your backend console, you should see a "PaymentIntent succeeded" log appear. Your Stripe account should show a $10 payment being made.

