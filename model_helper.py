from operator import attrgetter
from functools import reduce
import pprint
# REFERENCE: https://github.com/tpike3/bilateralshapley


class ModelCalculator():
    @staticmethod
    def compute_total(attribute):
        def helper(model):
            def reducer(accum, agent):
                # if attribute == "utility" and getattr(agent, attribute) > 200:
                #     print(accum, getattr(agent, attribute))
                return accum + getattr(agent, attribute)

            agents = model.schedule.agents
            return reduce(reducer, agents, 0)

        return helper


class CoalitionHelper():
    flag = 0
    '''Helper to form coalitions for agents'''

    def __init__(self, agents, id_key, power_key, pref_key, util_key,
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
        self.agents = agents
        self.id_key = id_key
        self.power_key = power_key
        self.pref_key = pref_key
        self.util_key = util_key
        self.efficiency = efficiency_parameter
        self.log_level = log_level

    def check_coalition(self, agent, other):
        # Condition to form coalition:
        #   bilateral shapley value > own power on both ends
        agent_power = getattr(agent, self.power_key)
        other_power = getattr(other, self.power_key)
        agent_pref = getattr(agent, self.pref_key)
        other_pref = getattr(other, self.pref_key)
        agent_util = getattr(agent, self.util_key)
        other_util = getattr(other, self.util_key)

        coal_power = (agent_power + other_power) * self.efficiency
        # ? TODO: check if this should be 100 or 1
        #     like in the preference repo
        # ? Well, they normalized the preference hmmm
        diff_pref = 100 - abs(agent_pref - other_pref)
        # Expected utility of the coalition
        inter_eu = coal_power * diff_pref

        # Determine bilateral shapley value for both agents
        # TODO: Check whether we should use power or expected utility
        # cbo_eu_1 = 0.5 * (agent_power + (inter_eu - other_power))
        # cbo_eu_2 = 0.5 * (other_power + (inter_eu - agent_power))
        cbo_eu_1 = 0.5 * (self.efficiency * agent_util + inter_eu)  # Shapley 1
        cbo_eu_2 = 0.5 * (self.efficiency * other_util + inter_eu)  # Shapley 2
        # coal_util = 0.5 * shap1 + 0.5 * shap2
        coal_util = 0.5 * ((agent_util + inter_eu) + (other_util + inter_eu))

        if cbo_eu_1 >= agent_util and cbo_eu_2 > other_util:
            # if a coalition increases both utilities
            # then this coalition is good enough
            # Coalition preference
            coal_pref = \
                ((agent_pref * agent_power + other_pref * other_power) /
                 (agent_power + other_power + 0.0000001))

            # if CoalitionHelper.flag < 5:
            #     CoalitionHelper.flag += 1
            #     # pp = pprint.PrettyPrinter(indent=4)
            #     # print(shap1, agent_util, inter_eu)
            #     # print(shap2, other_util, inter_eu)
            #     # pp.pprint(vars(agent))
            #     # pp.pprint(vars(other))
            #     print(agent_power, agent_pref, other_power, other_pref, flush=True)
            #     print((agent_pref * agent_power + other_pref * other_power), flush=True)
            #     print((agent_power + other_power + 0.0000001), flush=True)
            #     print(coal_pref, flush=True)
            #     print("\n", flush=True)
            # print("\n\n\n\n\n\n\n\n")

            return {
                "coal_power": coal_power,
                "coal_util": coal_util,
                "coal_pref": coal_pref,
                "utility_1": cbo_eu_1,
                "utility_2": cbo_eu_2,
            }

        return None

    def get_neighbor(self, agent_list, agent, model=None,
                     neighbor_type=0, talk_span=0):
        """Generate list of neighbor of the "agent"

        Args:
            agent_list ([list]): list of agent to check
            agent ([object]): current agent
            neighbor_type ([int]): type of neightbor to get for each cit
                0: treats all cits as neighbor
                1: direct neighbor
                2: small world network # TODO
                3: talk span in km
                (default: {0})
        """
        pp = pprint.PrettyPrinter(indent=2)
        if self.log_level >= 2:
            print(f"Getting neightbor list, type {neighbor_type}", flush=True)

        if neighbor_type == 0:
            # Make a copy of the old list
            neightbor_list = agent_list[:]
        if neighbor_type == 1:
            # ! Make sure this is an mesa_geo model!
            neightbor_list = model.grid.get_neighbors(agent)
        if neighbor_type == 3:
            neightbor_list = model.grid.get_neighbors_within_distance(
                agent, talk_span)

        if self.log_level >= 3:
            print("Got result: ", flush=True)
            pp.pprint(neightbor_list)

        return neightbor_list

    def form_coalition(self, model=None,
                       neighbor_type=0, talk_span=0):
        """Return a list of coalition from given agent list
        Args:
            model {Object} -- Mesa model object {default: {None}}
            neighbor_type {int} -- type of neightbor to get for each cit
                0: treats all cits as neighbor
                1: direct neighbor
                2: small world network # TODO
                3: talk span in km
                (default: {0})
        """

        pp = pprint.PrettyPrinter(indent=2)
        if self.log_level >= 2:
            print("Gathering potential coalitions", flush=True)

        # Map of potential coalition of each agent where
        #   key: agent_id
        #   value: potential coalition of that agent
        pot_coal_dict = {}
        id_of = attrgetter(self.id_key)

        # Try to form every possible coalition
        # This is technically an agent sending message out to others
        for agent in self.agents:
            # Check from one agent to another
            agent_id = id_of(agent)
            neighbor_list = self.get_neighbor(
                self.agents, agent, model, neighbor_type, talk_span)

            for other in neighbor_list:
                if id_of(other) == id_of(agent):
                    continue
                coalition_result = self.check_coalition(agent, other)

                # Coalition is good enough
                if coalition_result is not None:
                    def add_pot_coal():
                        pot_coal_dict[agent_id] = {"other_id": id_of(other)}
                        pot_coal_dict[agent_id].update(coalition_result)

                    coal_util = coalition_result['coal_util']
                    coal_pref = coalition_result['coal_pref']

                    # No possible mate yet
                    if agent_id not in pot_coal_dict:
                        add_pot_coal()
                    else:
                        pot_coal = pot_coal_dict[agent_id]
                        # or coalition with higher expected util
                        condition_1 = pot_coal["coal_util"] < coal_util
                        # or coalition with same expected utility
                        # but with closer pref
                        # that is: old coal pref > new coal pref
                        condition_2 = (pot_coal["coal_util"] == coal_util and
                                       pot_coal["coal_pref"] > coal_pref)
                        if condition_1 or condition_2:
                            add_pot_coal()

        if self.log_level >= 3:
            print("Potential coalition list", flush=True)
            pp.pprint(pot_coal_dict)

        if self.log_level >= 2:
            print("Forming coalitions", flush=True)

        # Check each possible coalition if both ends like the coalition
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

                # ? worried that the utility_1 won't match with agent?
                # ? no problem!
                # ? Since coal_info comes with agent, problem is solved!

                # ! Note: We don't update agents attributes here
                # ! to avoid side effect

                # Remove the item from possible coalition list
                #   to avoid duplicates
                # ? This only works if an agent can't be in
                # ? more than 1 coalition
                formed_list += [agent_id, other_id]

        if self.log_level >= 2:
            print("Finish forming coalitions, returning", flush=True)

        return coalition_list
