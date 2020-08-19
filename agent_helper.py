# TODO: Generalize this later
class AgentHelper():
    @staticmethod
    def update_cit_coalition_attrs(agent, pending_coalition):
        # Update attributes every step after cits coalitions are formed
        agent.own_pref = pending_coalition['coal_pref']
        agent.type = 2   # Change type to CBO
        agent.cbo_pref = pending_coalition['coal_pref']
        agent.power *= agent.efficiency_parameter

        # First agent on the coalition (initiative citizen)
        if agent.unique_id == pending_coalition['id_1']:
            agent.utility = pending_coalition['utility_1']
            agent.cbo_utility = pending_coalition['utility_1']
            agent.cbo_power = pending_coalition['coal_power']

            # They also become a sh
            # * Only the initiative citizen becomes the stake holder
            # *   since they are the one who sent out messages in the first
            agent.can_negotiate_sh = True

            # Update sh stuff
            agent.sh_pref = pending_coalition['coal_pref']
            agent.sh_power = pending_coalition['coal_power']
            agent.sh_utility = pending_coalition['utility_1']
        else:
            agent.utility = pending_coalition['utility_2']
            agent.cbo_utility = pending_coalition['utility_2']
            # The first citizen already consumed all the power
            agent.cbo_power = 0

    @staticmethod
    def update_sh_coalition_attrs(agent, pending_coalition):
        # Update attributes every step after stakeholder coalitions are formed
        agent.own_pref = pending_coalition['coal_pref']
        agent.sh_pref = pending_coalition['coal_pref']

        agent.sh_cbo_pref = pending_coalition['coal_pref']
        agent.sh_cbo_power = pending_coalition['coal_power']

        # First agent on the coalition (initiative citizen)
        if agent.unique_id == pending_coalition['id_1']:
            agent.sh_utility = pending_coalition['utility_1']
            agent.sh_cbo_utility = pending_coalition['utility_1']
        else:
            agent.sh_utility = pending_coalition['utility_2']
            agent.sh_cbo_utility = pending_coalition['utility_2']
            # The first stakeholder already consumed all the power
            agent.sh_cbo_power = 0

    @staticmethod
    def update_regulator_coalition_attrs(agent, pending_coalition):
        # Update attributes every step after stakeholder coalitions are formed
        agent.own_pref = pending_coalition['coal_pref']
        agent.regulator_pref = pending_coalition['coal_pref']

        agent.regulator_cbo_pref = pending_coalition['coal_pref']
        agent.regulator_cbo_power = pending_coalition['coal_power']

        # First agent on the coalition (initiative citizen)
        if agent.unique_id == pending_coalition['id_1']:
            agent.regulator_utility = pending_coalition['utility_1']
            agent.regulator_cbo_utility = pending_coalition['utility_1']
        else:
            agent.regulator_utility = pending_coalition['utility_2']
            agent.regulator_cbo_utility = pending_coalition['utility_2']
            # The first citizen already consumed all the power
            agent.regulator_cbo_power = 0
