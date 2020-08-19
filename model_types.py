from typing import Union, List

from cit_agent import CitAgent
from sh_agent import StakeholderAgent
from regulator_agent import RegulatorAgent

CBOable_Agent = Union[CitAgent, StakeholderAgent, RegulatorAgent]
CBOable_Agent_List = Union[List[CBOable_Agent],
                           List[CitAgent],
                           List[StakeholderAgent],
                           List[RegulatorAgent]]

All_Agent_Type = Union[CitAgent, StakeholderAgent, RegulatorAgent]
