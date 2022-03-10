import React from 'react';
import {
    BrowserRouter as Router,
    Switch,
    Route,
    Link
} from "react-router-dom";
import './App.css';
import Game from './pages/Game';
import Play from './pages/Play';

function App() {
  return (
    <div className="App">
          <Router>
              <ul>
                  <li>
                      <Link to="/">Home</Link>
                  </li>
                  <li>
                      <Link to="/host">Create Game</Link>
                  </li>
                  <li>
                      <Link to="/play">Join Game</Link>
                  </li>
              </ul>

              <hr />
              {/* A <Switch> looks through its children <Route>s and renders the first one that matches the current URL. */}
              <Switch>
                  <Route path="/host">
                      <Game />
                  </Route>
                  <Route path="/play">
                      <Play />
                  </Route>
              </Switch>
          </Router>      
    </div>
  );
}

export default App;

