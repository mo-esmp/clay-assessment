# clay-assessment

Video of testing lives in [asset](https://github.com/mo-esmp/clay-assessment/tree/main/assets) folder.

The project consists of 3 layers:
- Domain project - All things related to the domain (entities, interfaces, ...)
- Implementation - All implementation related to domain interfaces and third parties
- WebApi project - Contains Web API and MQTT gateway

## Technologies and libraries
- SQL Server for the database (for saving lock access logs, noSQL would be a better option) 
- Entity framework for the data access (makes development much faster and easier. For performance scenarios, Dapper could be an option, however, considering performance tips like using `AsNoTracking`, `Custom Select`, `Pooled DbContext`, and profiling complex queries, ... can improve performance)
- Orleans for handling lock request access throw API request or physical lock. Why Orleans:"
  - Each grain (actor) can represent a lock 
    - A grain processes request access one by one
    - A grain act as a cache and data of the lock is available in memory and no need for another component for caching
    - Lock access request history can be saved in the background process of grain and no need to block the response of lock access request for saving history
    - Need for an extra component like a queue to process lock access request history, each grain has a state and on grain shutdown histories can be persisted in a storage
    - When there is no access request for a lock, the activated grain will shutdown and memory will be freed
  - Clustering Orleans is easy
  - Orleans has built-in features like load-balancing, event sourcing, and streaming they are handy in some scenarios
- Dynamic Authorization for avoiding hardcoding roles on source code (library developed by me)

The architecture of the project is traditional N-Layers, however, in a real-life scenario it can break into a microservices architecture, for example:
- Separate service for Web API
- Separate Service for MQTT
- Separate Service for Orleans
- Separate Service for user management
- Separate Service for admin dashboard
And each service can be scaled based on the traffic

For the domain section, my design could be better (having the least knowledge in this area). Inside codes, I left some comments and explained what would be an alternative or more appropriate way to tackle the problem.

## How to test
- Adjust connection string in `appsettings.Development.json`
- Project can be accessed with:
  - `http://localhost:5000/` - Home Page
  - `http://localhost:5000/swagger` - API documentation
- Ports: 5000 http, 5001 https, 5002 mqtt
- To test MQTT, use `LockId` as `ClientId`, `Username`, `Subscription` and send JWT token (`{ jwtToken: "" }`) as the message payload
- After running `WebApi` project some sample data will be inserted into the database ([DbDataSeeder.cs](https://github.com/mo-esmp/clay-assessment/blob/main/src/SmartLock.Implementation/Data/DbDataSeeder.cs))
- No need for assigning roles to users (in the video it is shown but not needed) and can be accessed via `http://localhost:5000/role`
- Integration tests rely on a physical database (couldn't use in memory database because Dynamic Authorization lib (need to fix this :/)
- There is no test for MQTT unfortunately
