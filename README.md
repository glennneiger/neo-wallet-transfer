# neo-wallet-transfer

This program is built to transfer Neo wallet file of native Neo client(neo-cli and neo-gui) which is ended with `.db3` and `.json` into other data storage(MongoDB and MySQL for now).

## Usage

1. Open the project with Visual Studio.

2. Restore NuGet packages.

3. Build and run.

4. Click "Config" to config the database connection and the wallet source.

5. Click "Start".

## Notice

1. Sync the blockchain first because some data maybe necessary in the transfer process.

2. An addtional `.json` wallet will be created if you are tranfering a `.db3` wallet, and the final result will be based on NEP6 Standard.

3. You can transfer multiple wallets in a time.

4. Don't use duplicated accounts, they will produce errors. And if so, you have to delete the whole db and start again.

5. This project is based on neo-v2.8.0 source code, and some part of which is changed in the privacy to achieve the goal.

6. No index is added to any of these databases.

7. The result message is based on the NEP6 wallet structure and just count how many records are processed properly while doesn't represent the real data structure in the database.