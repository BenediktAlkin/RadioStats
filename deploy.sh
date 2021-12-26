# deploys the latest version to a fresh VM

# install tools
sudo apt-get install wget
sudo apt-get install unzip
sudo apt-get install tmux -y

# download latest release
wget https://github.com/BenediktAlkin/RadioStats/releases/latest/download/RadioStatsTweeter.zip
unzip RadioStatsTweeter.zip -d RadioStatsTweeter
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
chmod +x RadioStatsTweeter/Tweeter
rm RadioStatsTweeter.zip

# notify user to create configs
echo TODO create config.yaml
echo TODO create mailer_config.yaml
echo TODO create tweeter_config.yaml