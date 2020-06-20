from operator import attrgetter

# REFERENCE: https://github.com/tpike3/bilateralshapley


class CoalitionHelper():
    '''Helper to form coalitions for agents'''

    def __init__(self, agents, id_key, power_key, pref_key,
                 efficiency_parameter=1.5):
        """Initialize coalition helper

        Args:
            agents (list): list of agent - usually mesa "self.schedule.agents"
            id_key (string): agent attribute to get its id
            power_key (string): agent attribute to get its power
            pref_key (string): agent attribute to get its preference
            efficiency_parameter (float, optional): defaults to 1.5.
              interesting coalition formations typically fall between > 1.0
              and < 2.0. 1.0 or less results in no incentive to join
              a coalition (i.e. no coalitions) and usually more than 2.0
              results in everyone joining the same coalition
        """
        self.agents = agents
        self.id_key = id_key
        self.power_key = power_key
        self.pref_key = pref_key
        self.efficiency = efficiency_parameter

    def check_coalition(self, agent, other):
        agent_power = getattr(agent, self.power_key)
        other_power = getattr(agent, self.power_key)
        agent_pref = getattr(agent, self.pref_key)
        other_pref = getattr(agent, self.pref_key)

        coal_power = (agent_power + other_power) * self.efficiency
        # ? TODO: check if this should be 100 or 1
        #     like in the preference repo
        # ? Well, they normalized the preference hmmm
        diff_pref = 100 - abs(agent_pref - other_pref)
        # Expected utility of the coalition
        pot_eu = coal_power * diff_pref

        # determine bilateral shapley value for both agents
        # TODO: Check whether we should use power or expected utility
        shap1 = 0.5 * (agent_power + (pot_eu - other_power))
        shap2 = 0.5 * (other_power + (pot_eu - agent_power))

        if shap1 > agent_power and shap2 > other_power:
            # if a coalition increases both utilities
            # then this coalition is good enough
            return coal_power, pot_eu
        return None, None

    def form_coalition(self):
        # Map of potential coalition of each agent
        pot_coal = {}
        id_of = attrgetter(self.id_key)

        for agent in self.agents:
            # Check from one agent to another
            agent_id = id_of(agent)
            for other in self.agents:
                # No point to check against itself
                if agent != other:
                    coal_power, pot_eu = self.check_coalition(agent, other)

                    # Coalition is sensible
                    if coal_power is not None:
                        # No possible mate yet
                        # or coalition with higher expected util
                        if agent_id not in pot_coal or \
                                pot_coal[agent_id]["coal_eu"] < pot_eu:
                            pot_coal[agent_id] = {
                                "other_id": id_of(other),
                                "coal_power": coal_power,
                                "coal_eu": pot_eu
                            }
