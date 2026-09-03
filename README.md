# Overview

Backend of my **thesis work**: payment facilitator and pass handling application for gyms.

Provides server side features for the application using [ASP.NET Core](http://asp.net/). Works in demo mode only.

The solution was initially scaffolded using [Jason Taylor's Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture) version 9.0.12, and later it was upgraded to .NET 10. Clean Architecture was chosen due to the scale of the application. The solution follows not only the structure of Clean Architecture but is principles as well: separation of concerns, maintainability, scalability, testability etc.

**First and second semester presentation can be found in [docs folder](./docs).** Unfortunately the final thesis presentation is *lost*.

Find more information about the **frontend** [here](https://github.com/maidics/gym-pass-handling-platform-frontend).

---

## Structure: Clean Architecture

### [Domain Layer](./src/Domain)

Contains self-contain objects and business logic:
- Entities
- Constants (e.g: [`Roles`](./src/Domain/Constants/Roles.cs))
- Enums
- Events
- Strings
- Value objects

### [Application Layer](./src/Application)

Orchestrates business flows with [Domain Layer](./src/Domain) business definitions and [Infrastructure Layer](./src/Infrastructure) services through interfaces.

- Common:
  - Behaviours: [MediatR](https://github.com/LuckyPennySoftware/MediatR) `PipelineBehaviours`
  - EmailModels
  - Exceptions
  - Extensions
  - Interfaces
  - Models:
    - [`ClientNotification`](./src/Application/Common/Models/ClientNotification.cs): contains a localized message for the client
    - [`Result`](./src/Application/Common/Models/Result.cs): used instead of throwing `Exception` objects for performance
  - Resources: localization `.resx` files
  - Scopes/[`CultureInfoScope`](./src/Application/Common/Scopes/CultureInfoScope.cs)
    - Used for changing the culture info for a scope in a thread-safe manner
    - Credit: [Roland Tóth](https://blog.rolandtoth.hu/cultureinfo-scope/)
  - Security
  - Settings
- Additional folders for features containing: `IRequest`, `IRequestHandler`, `INotificationHandler` implementations and DTOs

### [Infrastructure Layer](./src/Infrastructure)

Contains code with external dependencies. Implements interfaces defined by [Application Layer](./src/Application):
- Common: extension methods
- Data:
  - [EF Core](https://learn.microsoft.com/en-us/ef/core/) logic for database handling
  - Configurations
  - Database seeding
  - Interceptors
  - [`QueryService`](./src/Infrastructure/Data/Queries/QueryService.cs): used for complex queries that cannot be performed via the [`IApplicationDbContext`](./src/Application/Common/Interfaces/IApplicationDbContext.cs) interface due to architecture constraints ([`ApplicationUser`](./src/Infrastructure/Identity/ApplicationUser.cs) exists in the [Infrastructure Layer](./src/Infrastructure))
- Email: local email service for demo purposes
- Identity: 
  - [`ApplicationUser`](./src/Infrastructure/Identity/ApplicationUser.cs)
  - [`IdentityService`](./src/Infrastructure/Identity/IdentityService.cs) for handling users and roles
- JWT: JSON Web Token service and settings class
- Localization: translator service
- Stripe: 
  - Payment service
  - Webhook handling service
  - Other services for handling multi-tenancy

### [Web Layer](./src/Web)

Application entry point from the web. Exposes endpoints and defines HTTP processing pipeline:
- Endpoints: defines HTTP endpoints for features and webhooks
- Infrastructure:
  - [`CustomExceptionHandler`](./src/Web/Infrastructure/CustomExceptionHandler.cs): middleware for uncaught `Exception` instances
  - Extension methods and more...
- Services:
  - [`CurrentUser`](./src/Web/Services/CurrentUser.cs): implements the [`IUser`](./src/Application/Common/Interfaces/IUser.cs) interface
  - [`ClientNotificationService`](./src/Web/Services/ClientNotificationService.cs):
    - Defines logic for writing and sending notifications
    - Backed by a `ConcurrentDictionary` for thread-safety
    - Notifications are sent out to each client connection for the user via `TypedResults.ServerSentEvents`
- JSON files for settings
- `Program.cs`: application entry

---

## Features

### [`Gym`](./src/Domain/Entities/Gym.cs)
- Update operations for `GymAdministrator` and `AppAdministrator`
- [`GymStatus`](./src/Domain/Enums/GymStatus.cs) change will result in notification sent to all gym employees via the [`IEmailService`](./src/Application/Common/Interfaces/IEmailService.cs) and the [`IClientNotificationSender`](./src/Application/Common/Interfaces/IClientNotificationService.cs) - which sends a real time notification to the frontend via `TypedResults.ServerSentEvents`

### [`Request`](./src/Domain/Entities/Request.cs)
- Built-in ticket system of the application
- [`Request`](./src/Domain/Entities/Request.cs) objects can be created by users in the following roles `User`, `GymStaff` and `GymAdministrator`
- Created ones are managed by `AppAdministrator` users. They can:
  - Reject them
  - Accept them. E.g: accepting a [`Request`](./src/Domain/Entities/Request.cs) with `GymCreation` [`RequestType`](./src/Domain/Enums/RequestType.cs) will result in creating the specified [`Gym`](./src/Domain/Entities/Gym.cs) and promoting the [`Request`](./src/Domain/Entities/Request.cs) creator to the `GymAdministrator` role inside a database transaction

### `PaymentIntent`
- Payments are handled through payment intents:
  1. Frontend sends a request to the backend to create a payment intent for a `User` & a [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs) => metadata will be attached to the Stripe `PaymentIntent`
  2. Backend responds with a [`PaymentIntentDto`](./src/Application/PaymentIntents/DTOs/PaymentIntentDto.cs) containing a client secret
  3. The acquired client secret will be used to display the required purchase components for the UI => the pass will be able to be purchased through this with bank card, Google/Apple Pay and more options
  4. The result of the payment will be sent be via a `StripeEvent` webhook to the backend which can serve accordingly
  5. In case of success the `User` will receive a [`GymMembershipPass`](./src/Domain/Entities/GymMembershipPass.cs) according to the purchased [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs) and the frontend will receive an event to notify the user an invalidate frontend caches // In case of error the `User` will receive notifications and an error will be logged

### [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs)
- Create, update and delete operations authorized to `GymAdministrator`
- Querying for all users
- Event handlers for successful and failed purchase webhooks coming from [Stripe](https://stripe.com/)
- A purchased [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs) will be used to create a [`GymMembershipPass`](./src/Domain/Entities/GymMembershipPass.cs) and other metadata contained by the [Stripe](https://stripe.com/) event will be used to assign it to the correct `User`

### [`GymMembership`](./src/Domain/Entities/GymMembership.cs)
- Holds purchased passes of an [`ApplicationUser`](./src/Infrastructure/Identity/ApplicationUser.cs) in the `User` role
- Also describes the [status](./src/Domain/Enums/GymMemberStatus.cs) of the user in the specified [`Gym`](./src/Domain/Entities/Gym.cs)
- The status of a membership can be changed by a user in a gym employee role (see: [`Roles`](./src/Domain/Constants/Roles.cs))
- This status can be `Banned` or `Active`
  - `Banned` status prevents the user from using their pass in the [`Gym`](./src/Domain/Entities/Gym.cs)
- In case of a status change the affected user is notified via email and client notification

### [`GymMembershipPass`](./src/Domain/Entities/GymMembershipPass.cs)
- Created by the backend according to the purchased [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs)
- It can be verified whether it is valid or not by a gym employee - or also used by them to start a gym session for the `User`

### [`GymEmployment`](./src/Domain/Entities/GymEmployment.cs)
- Holds values of a `GymStaff` or `GymAdministrator` user's employment in a [`Gym`](./src/Domain/Entities/Gym.cs)
- These objects are handled by the backend

### [`GymContactInfo`](./src/Domain/Entities/GymContactInfo.cs)
- CRUD operations for the [`GymContactInfo`](./src/Domain/Entities/GymContactInfo.cs) entity
- Describes [`Gym`](./src/Domain/Entities/Gym.cs) contact information such as email or [`PhoneNumber`](./src/Domain/ValueObjects/PhoneNumber.cs)

### [`ApplicationUser`](./src/Infrastructure/Identity/ApplicationUser.cs) & [`UserProfile`](./src/Domain/Entities/UserProfile.cs)
- Identity features are hand-built instead of using the Identity package features for education purposes:
  - Registration
  - Role handling
  - Password reset
  - etc.

---

## Technologies
- [ASP.NET Core](https://asp.net)
- [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [Stripe](https://stripe.com/) & [Stripe Connect](https://stripe.com/connect)
- [MediatR](https://github.com/LuckyPennySoftware/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/en/latest/)
- [NUnit](https://nunit.org/)
- [TestContainers](https://dotnet.testcontainers.org/)
- [Shouldly](https://github.com/shouldly/shouldly)
- [Respawn](https://github.com/jbogard/respawn)

---

## Testing

Covers unit tests for the [Domain Layer](./src/Domain) and [Application Layer](./src/Application). Also has functional testing of the [Application Layer](./src/Application) - this covers all business flows. 

Due to the size and time constraints of this thesis, the integration tests are left out of testing.

---

## Run the app

- Application requires .NET version: 10.0.100
- Running the app also requires API keys
- API keys are loaded from secrets.local.json in [Web project root](./src/Web) for **simplicity**
- Database type can be set to `InMemory` or `SQL` in appsettings.Development.json
- If SQL DbType is set the database will be created automatically using the given connection string in appsettings.Development.json

### Prerequisites
- Stripe Secret Key & webhook secret (from [StripeDashboard](https://dashboard.stripe.com/apikeys))
- Stripe Webhook endpoint url: https://localhost:5001/api/Webhooks/Stripe

**Add the secrets.local.json file with the following structure using your own API keys:**

<pre>
{
    "Stripe": {
        "Key": "insert_key",
        "WebhookSecret": "insert_key"
    },
    "Jwt": {
        "Key": "JG3Auz1bttDL8k9AX8PVC4LByiAveG0i+IoeFbSdCc8="
    }
}
</pre>

### Run app with Payments (test mode only)
- Download [Stripe CLI](https://github.com/stripe/stripe-cli/releases/tag/v1.35.1)
- in terminal run: stripe login --interactive
- provide tes API key: sk_test_...
- run: stripe listen --forward-to https://localhost:5001/api/Webhooks/Stripe --skip-verify
- after this payment related webhooks are forwarded to the correct endpoint to fulfill purchases

### Run automatic tests:
- Domain.UnitTests & Application.UnitTests do not require anything specific
- Application.FunctionalTests require Docker engine running (tested via SQL & Stripe containers) & appsettings.Testing.json: "DbType": "SQL"
- secrets.local.json is not required for testing
