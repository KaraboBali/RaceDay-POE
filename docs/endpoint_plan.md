# RaceDay API Endpoint Plan
**PROG6212 POE — Part 1, Section B**

This table covers every endpoint the RaceDay API will expose in Part 2, based on the ERD in `raceday_erd.png` and the two system roles (Organiser, Participant).

## Authentication

| Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Creates a new user account with a hashed password | None (public) | `{ name, email, password, role }` | 201 Created – user id + role; 400 Bad Request – validation failure; 409 Conflict – email already registered |
| POST | /api/auth/login | Validates credentials and issues a JWT | None (public) | `{ email, password }` | 200 OK – JWT token + role; 401 Unauthorized – invalid credentials |

## User Profile

| Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Returns the logged-in user's own profile | Any (logged in) | None | 200 OK – user profile (no password hash); 401 Unauthorized |
| PUT | /api/users/me | Updates the logged-in user's own name/email | Any (logged in) | `{ name, email }` | 200 OK – updated profile; 400 Bad Request |
| GET | /api/users/{id} | Returns any user's profile by id (so Organisers can identify participants) | Organiser | None | 200 OK – user profile; 403 Forbidden – not an Organiser; 404 Not Found |

## Events

| Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events | Creates a new event owned by the logged-in Organiser | Organiser | `{ name, date, location, description }` | 201 Created – event record; 400 Bad Request |
| GET | /api/events | Lists all events (browsable by everyone) | Any (logged in) | None | 200 OK – array of events |
| GET | /api/events/{id} | Returns a single event's details | Any (logged in) | None | 200 OK – event record; 404 Not Found |
| PUT | /api/events/{id} | Updates an event | Organiser | `{ name, date, location, description }` | 200 OK – updated event; 404 Not Found |
| DELETE | /api/events/{id} | Deletes an event | Organiser | None | 204 No Content; 404 Not Found |

## Categories

| Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events/{eventId}/categories | Adds a category to an event | Organiser | `{ name, distanceKm }` | 201 Created – category record; 404 Not Found (event doesn't exist) |
| GET | /api/events/{eventId}/categories | Lists categories for an event | Any (logged in) | None | 200 OK – array of categories |
| PUT | /api/categories/{id} | Updates a category | Organiser | `{ name, distanceKm }` | 200 OK; 404 Not Found |
| DELETE | /api/categories/{id} | Deletes a category | Organiser | None | 204 No Content; 404 Not Found |

## Routes

| Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/events/{eventId}/route | Creates the route for an event | Organiser | `{ description, distanceKm, mapUrl }` | 201 Created – route record; 404 Not Found (event doesn't exist); 409 Conflict (route already exists for this event) |
| GET | /api/events/{eventId}/route | Returns the route for an event | Any (logged in) | None | 200 OK – route record; 404 Not Found |
| PUT | /api/events/{eventId}/route | Updates the route for an event | Organiser | `{ description, distanceKm, mapUrl }` | 200 OK; 404 Not Found |

## Event Enrolments

| Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments | Enrols the logged-in Participant in a category | Participant | `{ categoryId }` | 201 Created – enrolment record; 409 Conflict (already enrolled in this category); 404 Not Found (category doesn't exist) |
| GET | /api/enrolments/me | Lists the logged-in Participant's own enrolments | Participant | None | 200 OK – array of enrolments |
| GET | /api/events/{eventId}/enrolments | Lists all enrolments for an event (organiser oversight) | Organiser | None | 200 OK – array of enrolments with participant names |
| DELETE | /api/enrolments/{id} | Cancels the logged-in Participant's own enrolment | Participant (must own the enrolment) | None | 204 No Content; 403 Forbidden; 404 Not Found |

## Results

| Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/results | Captures a result for a completed enrolment | Organiser | `{ enrolmentId, finishTime, position }` | 201 Created – result record; 404 Not Found (enrolment doesn't exist); 409 Conflict (result already captured) |
| GET | /api/results/me | Returns the logged-in Participant's own result history | Participant | None | 200 OK – array of results |
| GET | /api/events/{eventId}/results | Returns all results for an event (leaderboard view) | Any (logged in) | None | 200 OK – array of results with participant names |
