# HPort Integration Test

This directory contains integration tests for the HPort project. The tests are designed to verify the functionality and reliability of HPort in various scenarios.

These tests use the real Hetzner Cloud API, to ensure that HPort interacts correctly with the Hetzner Cloud services.
This means that running these tests will incur costs associated with the use of Hetzner Cloud resources.

## Prerequisites

Before running the tests, you need to:
1. Set up a new Hetzner Cloud project.
2. Obtain an API token for the project.
3. Write the API token in a file called `HETZNER_TOKEN`.

Ensure that you **do not** use your main Hetzner Cloud project for these tests,
as in the end **all** servers will be deleted to ensure no ongoing costs.

## Test Context

The integration test essentially runs two main scenarios:
1. Single Container Scenario (found in `SingleContainerTests.cs`)
   1. Create a new NGINX container on a Hetzner Cloud server.
   2. Verify that the container is running and accessible.
   3. Clean up the container and delete the server.
2. Multiple Containers Scenario (found in `MultipleContainersTests.cs`)
   1. Create a new NGINX container on a Hetzner Cloud server.
   2. Create a second NGINX container on the same server.
   3. Verify that both containers are running and accessible.
   4. Delete the first container and verify that the second container is still running.
   5. Clean up the second container and delete the server.