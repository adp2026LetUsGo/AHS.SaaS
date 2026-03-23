# tests/Cells/ColdChain/AHS.Cell.ColdChain.Tests/BDD/Features/ColdChainOracle.feature
@REQ-001 @GxP
Feature: Logistics Oracle Risk Assessment
  As a quality officer
  I want the Oracle to calculate logistics risk accurately
  So that I can protect pharmaceutical shipments

  Scenario: Passive insulation triggers base penalty
    Given a pharmaceutical shipment with setpoint 2–8°C
    And the shipment uses "Passive" insulation
    And the route category is "Low"
    When the Oracle calculates the risk
    Then the risk breakdown should include a passive penalty of 15%
    And the final risk score should exceed the weighted score
