# Pantry inventory

Nook's Pantry tracks household stock across Pantry, Fridge, Freezer, Cleaning, Bathroom, Pet, Medicine, and Other storage locations.

Items can be added manually with quantity, unit, category, optional expiry date, low-stock threshold, and notes. Low-stock items can be sent back to Shopping.

## Receipt scanning

Receipt scanning is a review-first import. A receipt photo is analysed on the server, candidate product lines are returned to the client, and the user can edit, include, or exclude each line before importing anything.

Supported receipt image types are JPEG, PNG, and WebP, up to 15 MB.

The API requires an OpenAI API key in server configuration:

```
OpenAI__ApiKey=<key>
```

The receipt model can optionally be overridden:

```
OpenAI__ReceiptModel=<model>
```

If no model is configured, the API uses `gpt-5.5`.

Receipt imports do not invent expiry dates. When a confirmed receipt item matches an active Shopping item, that Shopping item is marked completed.
