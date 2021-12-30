# deploys the latest version to a fresh VM

# install python
sudo apt-get install python3-pip -y
pip3 install --upgrade
pip3 install matplotlib

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

# notify user to create configs
echo TODO create config.yaml
echo TODO create mailer_config.yaml
echo TODO create tweeter_config.yaml
