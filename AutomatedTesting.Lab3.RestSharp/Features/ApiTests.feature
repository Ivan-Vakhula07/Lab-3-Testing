Feature: Web API Automation Testing using RestSharp
  As a Software QA Engineer
  I want to test REST API endpoints using RestSharp
  So that I can verify full CRUD operations and public API features

  Scenario: Perform full CRUD cycle on Restful-Booker API
    Given I have access to "https://restful-booker.herokuapp.com"
    When I request an authentication token via POST /auth
    And I create a new booking for "Ivan Vakhula" via POST /booking
    And I fetch the created booking details via GET /booking/{id}
    And I update the booking last name to "UpdatedLastName" via PUT /booking/{id}
    Then I delete the booking via DELETE /booking/{id}

  Scenario: Verify Public API integration (Cat Facts)
    Given I have access to "https://catfact.ninja"
    When I send a GET request to "/fact"
    Then I should receive an HTTP 200 OK status code and a non-empty fact