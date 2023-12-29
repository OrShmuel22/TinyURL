
# TinyURL Backend Service

## Overview

The TinyURL Backend Service is a URL shortening application developed in C#. This service allows users to shorten lengthy URLs into more manageable and shareable forms. The shortened URLs can then be expanded back to their original form. This service is especially useful for social media sharing, marketing, and managing large URLs.

## Features

1. **URL Shortening**: Converts a long URL into a shorter, more manageable version.
2. **URL Expansion**: Reverts a shortened URL back to its original long URL.
3. **Custom Memory Caching**: Implements an efficient caching mechanism for quick retrieval of URLs.
4. **MongoDB Integration**: Utilizes MongoDB for persistent storage of URL mappings.
5. **Base62 Encoding**: Employs Base62 encoding for generating the shortened URLs.
6. **Configurable Settings**: Provides customizable settings for database connection, URL shortening, and caching.

## Components

### 1. UrlShorteningService
- Core service for shortening and expanding URLs.
- Validates URLs, checks cache, queries the database, and handles the generation of shortened URLs.

### 2. CustomMemoryCache
- A custom memory caching system that stores recently accessed URLs.
- Optimizes performance by reducing database queries.

### 3. Base62
- Implements Base62 encoding and decoding.
- Ensures the short URLs are compact and unique.

### 4. UrlController
- The API controller handling HTTP requests.
- Provides endpoints for shortening and expanding URLs, and redirecting short URLs.

## Configuration

- `appsettings.json` contains configurable parameters like MongoDB connection details, base URL for shortening, and cache capacity.

## How to Use

1. **Shorten a URL**: Send a POST request to `/url/shorten` with the original URL in the request body.
2. **Expand a URL**: Send a GET request to `/url/expand/{shortUrl}` with the short URL code.
3. **Redirect Short URL**: Access a short URL directly to get redirected to the original URL.

---

*End of README*

## Custom Memory Cache

### Functionality
The cache in the TinyURL Backend Service is designed to store recently accessed URLs, enhancing retrieval speed and reducing the need for frequent database queries.

### Key Features
1. **LRU (Least Recently Used) Strategy**: Implements an eviction policy where the least recently used items are removed first upon reaching full capacity.
2. **Concurrent Access**: Utilizes `ConcurrentDictionary` for thread-safe operations, suitable for a multi-threaded environment.
3. **Doubly Linked List**: Maintains the order of items based on usage, facilitating efficient additions and deletions.

## Cache Size-Limitation Approach

In the design of the custom memory cache for the TinyURL Backend Service, I implemented a size-limitation approach based on fixed capacity. This decision aimed to balance memory efficiency with performance.

### Approach:
- **Fixed Capacity**: The cache has a predefined maximum capacity, limiting the number of items it can hold.

### Why This Approach:
1. **Memory Efficiency**: Limits memory usage, important in resource-constrained environments.
2. **Simplicity and Predictability**: Straightforward and predictable in terms of memory usage.

### Advantages:
1. **Prevents Memory Bloat**: Avoids memory leaks or bloat in an unbounded cache.
2. **Fast Lookup**: Ensures quick access for frequently accessed data.

### Disadvantages:
1. **Potential for Cache Misses**: If the cache is too small, it might lead to increased cache misses.
2. **Static Allocation**: Doesn't allow dynamic resizing based on workload, which could be inefficient or insufficient depending on the situation.

This approach was chosen for its balance between performance and resource usage, with the key being to size the cache appropriately based on expected workload and system resources.
