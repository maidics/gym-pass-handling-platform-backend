# FitPass

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) version 9.0.12.

# API keys

The API keys are loaded from secrets.local.json in Web project root. 

## Prerequisites
- Stripe Secret Key & webhook secret (from [StripeDashboard](https://dashboard.stripe.com/apikeys))

**Add the following structure with your own API keys to access Stripe & Jwt services:**

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