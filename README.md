# Liv'In Paris

Desktop application for a home-cooked meal delivery platform in Paris. Neighbourhood cooks publish dishes, customers order them, and deliveries are routed through the Paris metro network modelled as a weighted graph.

Built in C# (.NET 8, Windows Forms) with a MySQL database, as an engineering school project at ESILV.

## Features

- **Accounts and roles**: sign-up and login, with customer, cook and admin roles (a user can be both customer and cook)
- **Customer side**: browse dishes, place orders, follow order history
- **Cook side**: publish dishes (quantity, production and expiry dates, price, diet), manage incoming orders
- **Admin side**: manage users and orders, view statistics
- **Delivery routing**: shortest path between the cook's and the customer's nearest metro stations
- **Graph analysis**: Welsh-Powell colouring of the customer/cook order graph
- **Export**: database content exported to XML

## Graph algorithms

The metro network is loaded from the database into a generic graph (`Graphe<T>`, `Noeud<T>`, `Lien<T>`), with line connections linked by `StationManager`.

| Algorithm | Use |
|---|---|
| Dijkstra | Shortest delivery path between two stations |
| Bellman-Ford | Shortest paths, alternative implementation |
| Floyd-Warshall | All-pairs distances |
| Welsh-Powell | Colouring of the order graph (`GrapheCommandes`) |

## Project structure

```
ProjetLivInParis/
├── ProjetLivInParis/          Windows Forms application
│   ├── Graphe_Générique.cs    Generic graph and path algorithms
│   ├── Noeud_Générique.cs     Graph node
│   ├── Lien_Générique.cs      Weighted edge
│   ├── Station.cs             Metro station
│   ├── StationManager.cs      Loads the metro network from MySQL
│   ├── GrapheCommandes.cs     Order graph and Welsh-Powell colouring
│   ├── interfaceBDD.cs        Database access layer
│   ├── ExportXML.cs           XML export
│   └── Form_*.cs              User interface screens
├── ProjetLivInParis_Tests/    xUnit test project
└── ProjetLivInParis.sln
```

## Getting started

Requirements are Windows, Visual Studio 2022 (or the .NET 8 SDK) and a local MySQL server.

1. Create a MySQL database named `livinparis` with the project tables (`identite`, `client`, `cuisinier`, `plat`, `commande` and the metro stations).
2. Adjust the connection string in `ProjetLivInParis/interfaceBDD.cs` if your MySQL server does not run on `127.0.0.1:3306` with the default credentials.
3. Open `ProjetLivInParis.sln` and run the `ProjetLivInParis` project.

## Tech stack

C# · .NET 8 · Windows Forms · MySQL (MySql.Data) · xUnit
