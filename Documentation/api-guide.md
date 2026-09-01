# API Guide

## Overview
This guide documents the REST API endpoints for the Social Media Simulator.

## Authentication

### Register
```
POST /api/auth/register
```

### Login
```
POST /api/auth/login
```

## Posts

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/posts` | POST | Create post |
| `/api/posts/{id}` | GET | Get post |
| `/api/posts/{id}` | DELETE | Delete post |

## Feed

```
GET /api/feed
```

Returns personalized feed with pagination.
