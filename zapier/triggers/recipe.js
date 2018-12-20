const subscribeHook = (z, bundle) => {
  // `z.console.log()` is similar to `console.log()`.
  z.console.log('console says hello world!');

  // bundle.targetUrl has the Hook URL this app should call when a recipe is created.
  const data = {
    type: 0,
    webhook: bundle.targetUrl,
    sendUpdates: true,
    webhookType: 1
  };

  // You can build requests and our client will helpfully inject all the variables
  // you need to complete. You can also register middleware to control this.
  const options = {
    url: 'https://humblebundlenotifications.azurewebsites.net/api/RegisterWebhook',
    method: 'POST',
    body: JSON.stringify(data)
  };

  // You may return a promise or a normal data structure from any perform method.
  return z.request(options)
    .then((response) => JSON.parse(response.content));
};

const unsubscribeHook = (z, bundle) => {
  // bundle.subscribeData contains the parsed response JSON from the subscribe
  // request made initially.
  const hookId = bundle.subscribeData.id;
  const type = bundle.subscribeData.type;

  // You can build requests and our client will helpfully inject all the variables
  // you need to complete. You can also register middleware to control this.
  const options = {
    url: `https://humblebundlenotifications.azurewebsites.net/api/DeleteWebhookById/${type}/${hookId}`,
    method: 'DELETE',
  };

  // You may return a promise or a normal data structure from any perform method.
  return z.request(options)
    .then((response) => JSON.parse(response.content));
};

const getRecipe = (z, bundle) => {
  // bundle.cleanedRequest will include the parsed JSON object (if it's not a
  // test poll) and also a .querystring property with the URL's query string.
  const recipe = {
    name: bundle.cleanedRequest.Bundle.Name,
    url: bundle.cleanedRequest.Bundle.URL,
    description: bundle.cleanedRequest.Bundle.Description,
    imageUrl: bundle.cleanedRequest.Bundle.ImageUrl,
    sections: bundle.cleanedRequest.Bundle.Sections,
    items: bundle.cleanedRequest.Bundle.Items,
    isUpdate: bundle.cleanedRequest.Bundle.IsUpdate
  };

  return [recipe];
};

const getFallbackRealRecipe = (z, bundle) => {
  // For the test poll, you should get some real data, to aid the setup process.
  const options = {
    url: `https://humblebundlenotifications.azurewebsites.net/api/LatestBundle/${bundle.inputData.type}`,
    method: 'GET'
  };

  return z.request(options)
    .then((response) => JSON.parse(response.content));
};

// We recommend writing your triggers separate like this and rolling them
// into the App definition at the end.
module.exports = {
  key: 'recipe',

  // You'll want to provide some helpful display labels and descriptions
  // for users. Zapier will put them into the UX.
  noun: 'Bundle',
  display: {
    label: 'New Bundle',
    description: 'Trigger when a HumbleBundle is added or updated'
  },

  // `operation` is where the business logic goes.
  operation: {

    // `inputFields` can define the fields a user could provide,
    // we'll pass them in as `bundle.inputData` later.
    inputFields: [
      {key: 'type', type: 'string', helpText: 'Which HumbleBundle type this should trigger on? Games=0, Books=1, Mobile=2, Software=3, Mixed=4'}
    ],

    type: 'hook',

    performSubscribe: subscribeHook,
    performUnsubscribe: unsubscribeHook,

    perform: getRecipe,
    performList: getFallbackRealRecipe,

    // In cases where Zapier needs to show an example record to the user, but we are unable to get a live example
    // from the API, Zapier will fallback to this hard-coded sample. It should reflect the data structure of
    // returned records, and have obviously dummy values that we can show to any user.
    sample: {
      Bundle: {
        Name: "Humble Indie Bundle 19",
        URL: "https://www.humblebundle.com/games/humble-indie-bundle-19",
        Description: "SUPERHOT, JYDGE, SOMA, and more – cross-platform, DRM-free, and on Steam!",
        ImageUrl: "https://humblebundle.imgix.net/misc/files/hashed/632a3bceaaede04cf3d0dee88cfaa7f473cc3fb8.png?auto=compress&h=630&w=1200&s=98e4c242fff7139eceb2bee5740b605b",
        Type: 0,
        Sections: [
          {
            Title: "Pay what you want!",
            Items: [
              {
                Name: "Halcyon 6: Lightspeed Edition"
              }
            ]
          },
          {
            Title: "Beat the Average!",
            Items: [
              {
                Name: "Keep Talking and Nobody Explodes"
              }
            ]
          },
          {
            Title: "Pay $14 or more to also unlock!",
            Items: [
              {
                Name: "SUPERHOT"
              },
              {
                Name: "$2 Humble Wallet credit for Monthly subscribers"
              }
            ]
          }
        ],
        Items: [
          {
            Name: "Halcyon 6: Lightspeed Edition"
          },
          {
            Name: "Keep Talking and Nobody Explodes"
          },
          {
            Name: "SUPERHOT"
          },
          {
            Name: "$2 Humble Wallet credit for Monthly subscribers"
          }
        ]
      },
      IsUpdate: false
    }
  }
};
