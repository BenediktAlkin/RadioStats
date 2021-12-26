sudo apt-get install wget
sudo apt-get install unzip
sudo apt-get install tmux -y

wget https://github.com/BenediktAlkin/RadioStats/releases/latest/download/RadioStatsTweeter.zip
unzip RadioStatsTweeter.zip -d RadioStatsTweeter
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
chmod +x RadioStatsTweeter/Tweeter

# setup configs
nano config.yaml
nano mailer_config.yaml
nano tweeter_config.yaml

# copy configs into program directory
cp config.yaml RadioStatsTweeter/config.yaml
cp mailer_config.yaml RadioStatsTweeter/mailer_config.yaml
cp tweeter_config.yaml RadioStatsTweeter/tweeter_config.yaml

# copy existing db into program directory
cp RadioStats.sqlite RadioStatsTweeter/RadioStats.sqlite

# run in new tmux session
tmux new
./RadioStatsTweeter/Tweeter
# detach from session with ctrl + b, d

