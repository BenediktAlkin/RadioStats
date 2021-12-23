sudo apt-get install wget
sudo apt-get install unzip
sudo apt-get install tmux -y

wget https://github.com/BenediktAlkin/RadioStats/releases/latest/download/RadioStatsTweeter.zip
unzip RadioStatsTweeter.zip -d RadioStatsTweeter
cd RadioStatsTweeter
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
chmod +x Tweeter

# setup configs
nano config.yaml
nano mailer_config.yaml
nano tweeter_config.yaml

# new tmux session
tmux new
# attach
# tmux a
./Tweeter
# detach from session with ctrl + b, d

