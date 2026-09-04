# RaceDay

RaceDay is a full-stack event management system for the South African road running, walking, and cycling community. Event Organisers can create and manage events, categories, routes, and results, while Participants can browse events, enter events, track their enrolments, and view their personal result history.

## Roles

* **Organiser** — can create, edit, and delete events, manage event categories and routes, capture participant results, and view all event enrolments.
* **Participant** — can create an account, browse events, enter an event by selecting a category, view their own enrolments, and track their own results.

## Part 1 — Planning (this submission)

This part contains the planning documents for the system, all inside `/docs`:

* `docs/raceday\_erd.png` — Entity Relationship Diagram (6 entities: Users, Events, Categories, Routes, EventEnrolments, Results)
* `docs/endpoint\_plan.md` — Full API endpoint plan covering Authentication, User Profile, Events, Categories, Routes, Enrolments, and Results
* `docs/raceday\_schema.sql` — SQL Server script creating the full schema with sample data (2 Organisers, 2 Participants, 3 Events, categories, and enrolments)

No API code has been written in this part, per the brief.

## CI/CD

!\[CI/CD passing](docs/cicd-success.png)

## Video

<!-- Add your unlisted YouTube walkthrough link here -->

[Part 1 video walkthrough](PASTE_YOUTUBE_LINK_HERE)

## Setup Instructions

<!-- Fill in once Part 2/3 add real setup steps, e.g. connection strings, running migrations, etc. -->

1. Clone the repository.
2. Open `docs/raceday\_schema.sql` in SQL Server Management Studio and run it against a clean database to create the schema and sample data.

