# deploys the latest version to a fresh VM

# install tools
sudo apt-get install -y wget
sudo apt-get install -y unzip
sudo apt-get install -y tmux

# download latest release
wget https://github.com/BenediktAlkin/RadioStats/releases/latest/download/RadioStatsTweeter.zip
unzip RadioStatsTweeter.zip -d RadioStatsTweeter
rm RadioStatsTweeter.zip
chmod +x RadioStatsTweeter/Tweeter

# some setup stuff such that no errors occour
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
apt-get install -y libgdiplus
apt-get install -y libc6-dev

# notify user to create configs
echo TODO create config.yaml
echo TODO create mailer_config.yaml
echo TODO create tweeter_config.yaml