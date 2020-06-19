from bilateralshapley import BSV

coalition = BSV(agents, power_attribute, preference_attribute,
                efficiency_parameter, agent_id, compromise_parameter, verbose)

# Show a list of how agents group together
print(coalition.result)

# Show a list of how agents grouped together and each groups power and preference attribute
print(coalition.result_verbose)

# Show a dictionary of each group, the agents within that group
# and each agents updated power and preference value based on their assimilation into the group
print(coalition.subresults)
