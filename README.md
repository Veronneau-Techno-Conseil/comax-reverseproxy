# Commun Axiom <img src="Logo.png" style="height: 1em" /> - Commons Ecosystem <img src="ReverseProxy.png" style="height: 1em" /> 

**Commun Axiom** (Comax) is an online and offline software suite designed for nonprofit organizations and small to medium-sized businesses to facilitate data governance and collaboration. Its goal is to make data sharing and collaboration accessible to everyone by combining the functionality of many different software sources into one easy-to-use, non-invasive platform that supports decentralization and co-hosting. 

MIT Licence
<br/>
[Site web](https://communaxiom.org/)
<br/>

**Reverse proxy**
RP is a lightweight kubernetes operator to create a reverse proxy layer either automatically by synchronizing with Comax Cloud Manager (Accounts) or manually through the user interface. The software, as it is, is not ready to be used as it lacks necessary security layer. 

## To run the software

Running the applications requires an active kubeconfig with access to a default namespace. This should be done on a local instance of kubernetes running on docker.

There are two applications that can be run. 

- Comax RP UI
    1. Make sure OIDC settings are set to a working Accounts server with valid clientid / secrets
    2. Make sure that CentralUrl points to a valid target
    3. Db configurations can be left at default but you should ensure that they are identical to those of Rp Operator as both application needs to access the same isntance of the database. 
- Comax RP Operator
    1. Make sure OIDC settings are set to a working Accounts server with valid clientid / secrets
    2. Make sure that CentralUrl points to a valid target
    3. Ensure that DNS is set to a valid local DNS if your backend services are pointing to domain names
