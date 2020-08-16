from typing import Union, List

from cit_agent import CitAgent
from sh_agent import StakeholderAgent

CBOable_Agent = Union[CitAgent, StakeholderAgent]
CBOable_Agent_List = Union[List[CBOable_Agent],
                           List[CitAgent],
                           List[StakeholderAgent]]

All_Agent_Type = Union[CitAgent, StakeholderAgent]
