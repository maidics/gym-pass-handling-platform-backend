# FitPass

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) version 9.0.12.

# API keys

The API keys are loaded from secrets.local.json in Web project root (src/Web/secrets.local.json). 

## Prerequisites
- Stripe Secret Key & webhook secret (from [StripeDashboard](https://dashboard.stripe.com/apikeys))
- Stripe Webhook endpoint url: https://localhost:5001/api/Webhooks/Stripe

**Add the secrets.local.json file with the following structure using your own API keys to access Stripe & Jwt services:**

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

## Run app with Payments
- Download [Stripe CLI](https://github.com/stripe/stripe-cli/releases/tag/v1.35.1)
- in cmd/ bash run: stripe login --interactive
- provide the sk_test_... API key
- run stripe listen --forward-to https://localhost:5001/api/Webhooks/Stripe --skip-verify
- after this payment related webhooks are forwarded to the correct endpoint to fulfill purchases

## Additional Notes:
- You must use SQL server as the database (.UseSqlServer) in Infrastructure/DependencyInjection if you want to run the tests - this also requires docker