from operator import attrgetter
from functools import reduce
import pprint

from typing import Dict, Union
from model_types import CBOable_Agent, CBOable_Agent_List
# REFERENCE: https://github.com/tpike3/bilateralshapley


'''NOTES'''
# sh: stakeholder
# cit: citizen


class ModelCalculator():
    @staticmethod
    def compute_total(attribute):
        def helper(model=None, agent_list=None):
            def reducer(accum, agent):
                return accum + getattr(agent, attribute)

            result = 0
            try:
                agents = agent_list if agent_list is not None   \
                    else model.schedule.agents
                # agents = model.schedule.agents
                result = reduce(reducer, agents, 0)
                # result = result if result == 0 else math.log(result)
            except AttributeError:
                # This means the agent is not a cit
                return 0
            return result

        return helper


class CoalitionHelper():
    '''Helper to form coalitions for agents'''

    def __init__(self, id_key, power_key, pref_key, util_key,
                 efficiency_parameter=1.5, log_level=0):
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
        self.id_key = id_key
        self.power_key = power_key
        self.pref_key = pref_key
        self.util_key = util_key
        self.efficiency = efficiency_parameter
        self.log_level = log_level

    def form_coalition(self, get_neighbors,
                       agent_list: CBOable_Agent_List,
                       ignored_list: CBOable_Agent_List):
        """Return a list of coalition from given agent list
        Args:
            get_neighbors: neighbor getter for agents
            ignored_list: list of citizen that should be ignored
        """

        # Map of potential coalition of each agent where
        #   key: agent_id
        #   value: potential coalition of that agent
        pot_coal_dict = {}
        id_of = attrgetter(self.id_key)

        # Filter out the ignored list
        old_len = len(agent_list)
        agent_list = [
            agent for agent in agent_list if agent not in ignored_list]
        self.print_log(
            2, "Gathering potential coalitions from {} agents out of {} agents"
            .format(len(agent_list), old_len))

        # Try to form every possible coalition
        # This is technically an agent sending message out to others
        for agent in agent_list:
            # Check from one agent to another
            agent_id = id_of(agent)
            neighbor_list = get_neighbors(agent, agent_list)

            for other in neighbor_list:
                if id_of(other) == id_of(agent):
                    continue
                coalition_result = self.get_cit_coalition(agent, other)

                # Coalition is good enough
                if coalition_result is not None:
                    def add_pot_coal():
                        pot_coal_dict[agent_id] = {"other_id": id_of(other)}
                        pot_coal_dict[agent_id].update(coalition_result)

                    utility_1 = coalition_result['utility_1']
                    coal_pref = coalition_result['coal_pref']

                    # No possible mate yet
                    if agent_id not in pot_coal_dict:
                        add_pot_coal()
                    else:
                        pot_coal = pot_coal_dict[agent_id]
                        # or coalition with higher expected util
                        condition_1 = pot_coal["utility_1"] > utility_1
                        # or coalition with same expected utility
                        # but with closer pref
                    # that is: old coal pref > new coal pref
                        condition_2 = (pot_coal["utility_1"] == utility_1 and
                                       pot_coal["coal_pref"] < coal_pref)
                        if condition_1 or condition_2:
                            add_pot_coal()
        self.print_log(3, "Potential coalition list", pot_coal_dict)

        # Check each possible coalition if both ends like the coalition
        self.print_log(2, "Forming coalitions")
        coalition_list = []
        formed_list = []    # List of agents already in coalition
        for agent_id, coal_info in pot_coal_dict.items():
            # First retrieve the potential coalition from other end
            other_id = coal_info['other_id']
            if other_id in formed_list:
                # No point of forming coalition with the same agent
                continue

            other_coal_info = pot_coal_dict.get(other_id)

            if other_coal_info is not None \
                    and agent_id == other_coal_info['other_id']:
                # If both coalition refer to each other
                #   then we form a new coalition
                new_coalition = {
                    "id_1": agent_id,
                    "id_2": other_id,
                }
                # Make sure to add other attributes too
                new_coalition.update(coal_info)
                # but remove the redundant key
                new_coalition.pop('other_id')
                coalition_list.append(new_coalition)

                # Remove the item from possible coalition list
                #   to avoid duplicates
                # ? This only works if an agent can't be in
                # ? more than 1 coalition
                formed_list += [agent_id, other_id]

        self.print_log(2, "Finish forming coalitions, returning")

        return coalition_list

    #
    #
    '''Helpers'''

    # Helper to check whether a coalition is possible between 2 agents
    def get_cit_coalition(self,
                          agent: CBOable_Agent,
                          other: CBOable_Agent) \
            -> Union[Dict[str, float], None]:
        # Condition to form coalition:
        #   bilateral shapley value > own power on both ends
        agent_power = getattr(agent, self.power_key)
        other_power = getattr(other, self.power_key)
        agent_pref = getattr(agent, self.pref_key)
        other_pref = getattr(other, self.pref_key)
        agent_util = getattr(agent, self.util_key)
        other_util = getattr(other, self.util_key)

        coal_power = (agent_power + other_power) * self.efficiency
        diff_pref = 100 - abs(agent_pref - other_pref)
        # Expected utility of the coalition
        inter_eu = coal_power * diff_pref

        # Determine bilateral shapley value for both agents
        # TODO: Check whether we should use power or expected utility
        # cbo_eu_1 = 0.5 * (agent_power + (inter_eu - other_power))
        # cbo_eu_2 = 0.5 * (other_power + (inter_eu - agent_power))
        cbo_eu_1 = 0.5 * (self.efficiency * agent_util + inter_eu)  # Shapley 1
        cbo_eu_2 = 0.5 * (self.efficiency * other_util + inter_eu)  # Shapley 2
        # coal_util = 0.5 * cbo_eu_1 + 0.5 * cbo_eu_2
        coal_util = 0.5 * ((agent_util + inter_eu) + (other_util + inter_eu))

        if cbo_eu_1 >= agent_util and cbo_eu_2 > other_util:
            # if a coalition increases both utilities
            # then this coalition is good enough
            # Coalition preference
            coal_pref = \
                ((agent_pref * agent_power + other_pref * other_power) /
                 (agent_power + other_power + 0.0000001))

            return {
                "coal_power": coal_power,
                "coal_util": coal_util,
                "coal_pref": coal_pref,
                "utility_1": cbo_eu_1,
                "utility_2": cbo_eu_2,
            }
        return None

    def check_sh_coalition(self, agent, other):
        # Condition to form coalition:
        #   bilateral shapley value > own power on both ends
        agent_power = getattr(agent, self.power_key)
        other_power = getattr(other, self.power_key)
        agent_pref = getattr(agent, self.pref_key)
        other_pref = getattr(other, self.pref_key)
        agent_util = getattr(agent, self.util_key)
        other_util = getattr(other, self.util_key)

        coal_power = agent_power + other_power
        coal_pref = \
            ((agent_pref * agent_power + other_pref * other_power) /
             (agent_power + other_power + 0.0000001))

        # Expected utility of the coalition
        # ! Need to double check this
        coal_util = coal_power * (100 - abs(coal_pref - coal_pref))

        # Determine bilateral shapley value for both agents
        cbo_eu_1 = (100 - abs(coal_pref - agent_pref)) * \
            (agent_power + (coal_power - agent_power)) * \
            (agent_power / (coal_power + 0.0000001))        # Shapley 1
        cbo_eu_2 = (100 - abs(coal_pref - agent_pref)) * \
            (other_power + (coal_power - other_power)) * \
            (other_power / (coal_power + 0.0000001))        # Shapley 2

        if cbo_eu_1 >= agent_util and cbo_eu_2 > other_util:
            # if a coalition increases both utilities
            # then this coalition is good enough
            # Coalition preference
            return {
                "coal_power": coal_power,
                "coal_util": coal_util,
                "coal_pref": coal_pref,
                "utility_1": cbo_eu_1,
                "utility_2": cbo_eu_2,
            }
        return None

    #
    #
    # Loggings
    def print_log(self, log_level, log_string, json_data=None):
        if self.log_level >= log_level:
            print(log_string, flush=True)
            if json_data is not None:
                pp = pprint.PrettyPrinter(indent=2)
                pp.pprint(json_data)
