# HumbleBundleBot

![](https://img.shields.io/badge/Built%20On-Azure%20Functions-blue.svg)
[![Build status](https://ironmoosedevelopment.visualstudio.com/Iron%20Moose%20Development/_apis/build/status/HumbleBundleBot%20CI)](https://ironmoosedevelopment.visualstudio.com/Iron%20Moose%20Development/_build/latest?definitionId=18)
![](https://ironmoosedevelopment.vsrm.visualstudio.com/_apis/public/Release/badge/94ed5e56-0dc7-4503-a43d-3f1f8a8240e1/1/1)
[![](https://img.shields.io/badge/Chat-On%20Discord-orange.svg)](https://discord.gg/A7NBpcC)
[![](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-%243-orange)](https://www.buymeacoffee.com/T2tZvWJ)

Scrapes HumbleBundle.com for new Bundles and posts new bundles to different webhooks whenever a new Bundle shows up.
Each Bundle can be sent to a different webhook, allowing easy organization of messages - in Discord, each channel can have webhooks associated with them for integrations like this.

Scraping is done once and results are sent to all relevant webhooks using Serverless Functions and Azure Queue, making the system extremely cheap to run and highly scalable.

![](https://s33.postimg.cc/3w0ux45wv/Capture.png)

![](https://i.postimg.cc/3JJQYcnb/image.png)

## How to generate a webhook

https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks

## Get started fast with our registration site

[https://humblebundlenotificationssite.azurewebsites.net](https://humblebundlenotificationssite.azurewebsites.net/)

## Changelog
`3/13/2019` - Thanks to [BallBrian](https://github.com/ballbrian), we now have a nice blazor powered registration site! Check it out here: [https://humblebundlenotificationssite.azurewebsites.net](https://humblebundlenotificationssite.azurewebsites.net/)

`1/8/2019` - In response to popular demand, you can now *optionally* register a [Humble Bundle Partner](https://www.humblebundle.com/partner) referral that will be attached to all messages for *your webhook only*. This means that if you're a streamer who runs your own Discord and want your audience to be able to throw some of their purchase money at you, you can signup with Humble Bundle and register your partner name here to make it happen!

Not a Partner and want to support me for developing this service? Use `cswendrowski` for the `Partner` value when signing up, as the example will show. Thank you!

`11/8/2018` - You can now opt-in to Developer Messages - we will only send these out in case of major outages to indicate when the Bot is down for maintenance and when it is back up. This message type is currently only valid for Discord webhooks.

`5/23/2018` - We now support the ability to register any webhook instead of just Discord webhooks.
If you register a Discord webhook the experience is still the same and you will receive Discord formatted messages.
If you have already registered a webhook before this date, your webhook registration has been defaulted to Discord.

If you register a webhook of type `RawJson` (1), you will receive a `BundleQueue` payload that looks similiar to the following:

```json
{
  "Bundle": {
    "Name": "Humble Indie Bundle 19",
    "URL": "https://www.humblebundle.com/games/humble-indie-bundle-19",
    "Description": "SUPERHOT, JYDGE, SOMA, and more – cross-platform, DRM-free, and on Steam!",
    "ImageUrl": "https://humblebundle.imgix.net/misc/files/hashed/632a3bceaaede04cf3d0dee88cfaa7f473cc3fb8.png?auto=compress&h=630&w=1200&s=98e4c242fff7139eceb2bee5740b605b",
    "Type": 0,
    "Sections": [
      {
        "Title": "Pay what you want!",
        "Items": [
          {
            "Name": "Halcyon 6: Lightspeed Edition"
          }
        ]
      },
      {
        "Title": "Beat the Average!",
        "Items": [
          {
            "Name": "Keep Talking and Nobody Explodes"
          }
        ]
      },
      {
        "Title": "Pay $14 or more to also unlock!",
        "Items": [
          {
            "Name": "SUPERHOT"
          },
          {
            "Name": "$2 Humble Wallet credit for Monthly subscribers"
          }
        ]
      }
    ],
    "Items": [
      {
        "Name": "Halcyon 6: Lightspeed Edition"
      }
      {
        "Name": "Keep Talking and Nobody Explodes"
      },
      {
        "Name": "SUPERHOT"
      },
      {
        "Name": "$2 Humble Wallet credit for Monthly subscribers"
      }
    ]
  },
  "IsUpdate": false
}
```

## API Docs

### How To Register a Webhook with a Partner link

*All requests to the endpoints require a header of `Content-Type: application/json`*

Make a HTTP POST request to https://humblebundlenotifications.azurewebsites.net/api/RegisterWebhook with a Body of the following format:

```
{
    "type": <Valid Bundle Type Code>,
    "webhook": "<YOUR DISCORD WEBHOOK URL>",
    "sendUpdates": <true|false>,
    "webhookType": <Valid Webhook Type Code>,
    "partner": "<YOUR PARTNER NAME OF CHOICE>"
}
```
The following Bundle types are valid:

| Type        | Type Code |
| ----------- | --------- |
| Games       | 0         |
| Books       | 1         |
| Mobile      | 2         |
| Software    | 3         |
| Mixed (Mixed Content Bundles)  | 4 |
| Developer Messages (Valid for Discord only)| 5  |
| All Bundles | 6         |

The following Webhook types are valid:

| Type        | Type Code |
| ----------- | --------- |
| Discord     | 0         |
| RawJson     | 1         |

Example of subscribing to the Games Bundle with a `Discord` webhook with a Partner link of me (Thanks if you do this!):

```json
{
    "type": 0,
    "webhook": "https://discordapp.com/api/webhooks/abcd123...",
    "sendUpdates": true,
    "webhookType": 0,
    "partner": "cswendrowski"
}
```

Webhook URLs are encrypted before they are stored.

A Partner link will add a header to the page indicating who referred the user to the bundle:
![](https://i.postimg.cc/Jzn62wPM/image.png)

It will also add a slider bar to the "Choose where your money goes section" where users can adjust how much the Partner receives (even down to nothing!)
![](https://i.postimg.cc/Kzy5FCrK/image.png)

#### How to find your Partner name

Are you signed up as a Partner and confused about what value to use when registering?

You can find your Partner name at on the [Partner Dashboard](https://www.humblebundle.com/partner/dashboard), under the "Active partner" section:
![](https://i.postimg.cc/k4zqBFGt/image.png)

## How To Register a Webhook without a Partner link

*All requests to the endpoints require a header of `Content-Type: application/json`*

Make a HTTP POST request to https://humblebundlenotifications.azurewebsites.net/api/RegisterWebhook with a Body of the following format:

```
{
    "type": <Valid Bundle Type Code>,
    "webhook": "<YOUR DISCORD WEBHOOK URL>",
    "sendUpdates": <true|false>,
    "webhookType": <Valid Webhook Type Code>
}
```

Example of subscribing to the Games Bundle with a `Discord` webhook:

```
{
    "type": 0,
    "webhook": "https://discordapp.com/api/webhooks/abcd123...",
    "sendUpdates": true,
    "webhookType": 0
}
```

Webhook URLs are encrypted before they are stored.

## How To Test a Registered Webhook
*All requests to the endpoints require a header of `Content-Type: application/json`*

This endpoint will lookup your registered webhook and, if it exists, send a recent Bundle of that type to your webhook. You can also specify which bundle you want if you just registered and want a current bundle.

Make a HTTP POST request to https://humblebundlenotifications.azurewebsites.net/api/TestWebhook with a Body of the following format:

```
{
    "type": <Valid Bundle Type Code>,
    "webhook": "<YOUR DISCORD WEBHOOK URL>",
    "webhookType": <Valid Webhook Type Code>,
    "bundleName": "<OPTIONAL: If included, will look for a particular bundle to send. Case insensitive>"
}
```

Example of testing a registered webhook for the Games Bundle with a `Discord` webhook:

```json
{
    "type": 0,
    "webhook": "https://discordapp.com/api/webhooks/abcd123...",
    "webhookType": 0
}
```

Specific bundle:

```json
{
    "type": 0,
    "webhook": "https://discordapp.com/api/webhooks/abcd123...",
    "webhookType": 0,
    "bundleName": "Super Cool Bundle"
}
```

## How to remove a registered Webhook

*All requests to the endpoints require a header of `Content-Type: application/json`*

Make a HTTP DELETE request to https://humblebundlenotifications.azurewebsites.net/api/DeleteWebhook with a Body of the following format:

```
{
    "type": <Valid Bundle Type Code>,
    "webhook": "<YOUR SUBSCRIBED WEBHOOK URL>",
    "webhookType": 0
}
```

