# move config/db from RadioStatsTweeter directory to home directory
mv RadioStatsTweeter/config.yaml config.yaml
mv RadioStatsTweeter/mailer_config.yaml mailer_config.yaml
mv RadioStatsTweeter/tweeter_config.yaml tweeter_config.yaml
mv RadioStatsTweeter/RadioStats.sqlite RadioStats.sqlite 

# delete old version
rm -rf RadioStatsTweeter
# download new version
wget https://github.com/BenediktAlkin/RadioStats/releases/latest/download/RadioStatsTweeter.zip
unzip RadioStatsTweeter.zip -d RadioStatsTweeter
chmod +x RadioStatsTweeter/Tweeter
rm RadioStatsTweeter.zip

# copy configs into program directory
mv config.yaml RadioStatsTweeter/config.yaml
mv mailer_config.yaml RadioStatsTweeter/mailer_config.yaml
mv tweeter_config.yaml RadioStatsTweeter/tweeter_config.yaml
# wget https://github.com/BenediktAlkin/RadioStats/releases/download/v0.1.3/RadioStats.sqlite
mv RadioStats.sqlite RadioStatsTweeter/RadioStats.sqlite
