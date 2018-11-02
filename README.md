# neo-wallet-transfer

This program is built to transfer Neo wallet file of native Neo client(neo-cli and neo-gui) which is ended with `.db3` and `.json` into other data storage(MongoDB and MySQL for now).

## Usage

1. Open the project with Visual Studio.

2. Restore NuGet packages.

3. Build and run.

4. Click "Config" to config the database connection and the wallet source.

5. Create a database called `neo` in MySQL if you want to use it.

6. Click "Start".

## Notice

1. Sync the blockchain first if some data maybe necessary in the transfer process, in the same way like neo-cli.

2. An addtional `.json` wallet will be created if you are tranfering a `.db3` wallet, and the final result will be based on NEP6 Standard.

3. You can transfer multiple wallets in a time.

4. Don't use duplicated accounts, they will produce errors. And if so, you have to delete the whole db and start again.

5. This project is based on neo-v2.8.0 source code, and some part of which is changed in the privacy to achieve the goal.

6. No index is added to any of these databases.

7. The result message is based on the NEP6 wallet structure and just count how many records are processed properly while doesn't represent the real data structure in the database.

## MySQL Table Structure

### neo.scrypt

Store the unique scrypt information of wallets.

SQL: `CREATE TABLE IF NOT EXISTS neo.scrypt ( uid INT AUTO_INCREMENT, n INT, r INT, p INT, PRIMARY KEY(uid))engine = innodb;`

Name in table | Type in table | Name in wallet | Type in wallet | Name in NEP6 | Type in NEP6 |
| - | - | - | - | - | - |
| uid | int | N/A | N/A | N/A | N/A |
| n | int | n | int | n | int |
| r | int | r | int | r | int |
| p | int | p | int | p | int |

### neo.contract

Store the contract information of every account.

SQL: `CREATE TABLE IF NOT EXISTS neo.contract ( script VARCHAR(1000), deployed TINYINT(1), PRIMARY KEY(script))engine = innodb;`

Name in table | Type in table | Name in wallet | Type in wallet | Name in NEP6 | Type in NEP6 |
| - | - | - | - | - | - |
| script | varchar(1000) | script | string | Script | byte[]
| deployed | tinyint(1) | deployed | boolean | deployed | bool |

### neo.wallet

Store the information of every wallet, referring whose scrypt information.

SQL: ``CREATE TABLE IF NOT EXISTS neo.wallet ( uid INT AUTO_INCREMENT, name VARCHAR(45), version VARCHAR(10), scrypt_id INT, password VARCHAR(45), extra VARCHAR(45), PRIMARY KEY(uid), CONSTRAINT `scrypt_id1` FOREIGN KEY (`scrypt_id`) REFERENCES `scrypt` (`uid`))engine = innodb;``

Name in table | Type in table | Name in wallet | Type in wallet | Name in NEP6 | Type in NEP6 |
| - | - | - | - | - | - |
| uid | int | N/A | N/A | N/A | N/A |
| name | varchar(45) | name | string | name | string |
| version | varchar(45) | version | string | version | Version |
| scrypt_id | int | N/A | N/A | N/A | N/A |
| password | varchar(45) | N/A | N/A | N/A | N/A |
| extra | varchar(45) | extra | JObject | extra | JObject |

### neo.account

Store the information of every account, referring whose contract information.

SQL: ``CREATE TABLE IF NOT EXISTS neo.account ( address VARCHAR(45), label VARCHAR(45), isDefault TINYINT(1), locked TINYINT(1), account_key VARCHAR(100), contract_script VARCHAR(1000), wallet_id INT, extra VARCHAR(45), PRIMARY KEY(address), CONSTRAINT `wallet_id1` FOREIGN KEY (`wallet_id`) REFERENCES `wallet` (`uid`), CONSTRAINT `contract_script1` FOREIGN KEY (`contract_script`) REFERENCES `contract` (`script`))engine = innodb;``

Name in table | Type in table | Name in wallet | Type in wallet | Name in NEP6 | Type in NEP6 |
| - | - | - | - | - | - |
| address | varchar(45) | address | string | ScriptHash | UInt160 |
| label | varchar(45) | label | string | Label | string |
| isDefault | tinyint(1) | isDefault | boolean | IsDefault | bool |
| locked | tinyint(1) | lock | boolean | Lock | bool |
| account_key | varchar(100) | key | string | nep2key | string |
| contract_script | varchar(1000) | N/A | N/A | N/A | N/A |
| wallet_id | int | N/A | N/A | N/A | N/A |
| extra | varchar(45) | extra | JObject | extra | JObject |

### neo.parameter

Store the relationships between contracts and parameters.

SQL: ``CREATE TABLE IF NOT EXISTS neo.parameter ( uid INT AUTO_INCREMENT, contract_script VARCHAR(1000), name VARCHAR(45), type VARCHAR(45), PRIMARY KEY(uid), CONSTRAINT `contract_script2` FOREIGN KEY (`contract_script`) REFERENCES `contract` (`script`))engine = innodb;``

Name in table | Type in table | Name in wallet | Type in wallet | Name in NEP6 | Type in NEP6 |
| - | - | - | - | - | - |
| uid | int | N/A | N/A | N/A | N/A |
| contract_script | varchar(1000) | N/A | N/A | N/A | N/A |
| name | varchar(45) | name | string | ParameterNames | string[] |
| type | varchar(45) | type | string | ParameterList | ContractParameterType[] |

## Mongo Structure

database(neo)->collection(wallet)->document(the whole json file)

## Screenshot

![Screenshot](https://i.loli.net/2018/11/02/5bdbfa9937b8a.png)