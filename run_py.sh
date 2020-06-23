# Use this script to switch conda environment if neccessary
eval "$(conda shell.bash hook)"
conda activate abm

python "${@:1}"