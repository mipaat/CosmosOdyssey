# CosmosOdyssey

This is a simple ASP.NET Core MVC web application, created as a test project to apply for a job proposal.

## Quick Setup
This application requires .NET 7 and a PostgreSQL database to run.  

The easiest way to get started is to use Docker and Docker Compose.  
You can refer to [this webpage](https://docs.docker.com/engine/install/) to install Docker.  
If you installed Docker Desktop, Docker Compose should have also been installed alongside it. If not, [this guide](https://docs.docker.com/compose/install/linux/) might help.

You can use the following commands to run this application using Docker Compose.  
(All of these should be run from the solution root directory, the same one that contains this README and the `docker-compose.yml` file.)

- Start everything: `docker compose up`
- Stop everything: `docker compose down`
- Start only the database: `docker compose up db`
- Start only the webapp: `docker compose up webapp`

You can append `-d` to the `docker compose up` commands to run them in the background.
If you don't, you'll probably need to press `Ctrl+C` when you want to stop the application.

Once started, the webapp should be accessible via at least one of the following URLs:
- [localhost:8080](http://localhost:8080)
- [127.0.0.1:8080](http://127.0.0.1:8080)
- [[::1]:8080](http://[::1]:8080)

## Features

The user can:
- See available routes between planets
- Filter and sort the route offers
- If logged in, make a reservation based on an offer
- (If the offer is expired, the reservation will not go through, of course)
- If logged in, view the reservations they've made in the past

If both the "From" and "To" query are specified in the route view, the system will also offer combinations of flights to complete the journey.  
It does not search very far for these connections, but should find most routes that could be of interest with the given dataset.

The "From", "To", and "Company" filters are case insensitive and will match any values that contain them as a substring.
For example, "aRT" would match "Earth".

The system fetches route offers from an API when the previous offers expire
or (just in case) when a certain time interval has passed.  
At regular intervals (1 hour), the system deletes expired price lists and their related information from the database,
except for the latest 15 price lists.

## Shortcomings of the system

The system doesn't stop users from reserving the same flight multiple times.  
Nor does it have any limit on reservations, because no such limit was provided by the API.  
That seemed to indicate that such a feature was not expected from this solution.

The system is coupled to the external API to quite a high extent. Even the database design is modelled after the API schema.  
This is not ideal, but seemed reasonable given the apparent scope of this assignment.

Most unfortunately, the API responses feature different IDs for entities that are likely meant to be the same.  
Even IDs that are consistent within one price list are different in the next one.  
It would be possible to ignore the IDs and treat entities with the same name as the same entity,
but although that could have worked within the context of this assignment, it did not seem like the right choice.  
The entire purpose of IDs is to **uniquely** identity an entity. If the API treats these entities as different, so should this system. Unnecessary data duplication felt like the lesser of two evils here.  
Additionally, the assignment specified that **all** the details provided by the API needed to be stored,
which presumably would include the IDs. If we de-duplicated the data based on names, handling these external IDs would become fairly clunky.
