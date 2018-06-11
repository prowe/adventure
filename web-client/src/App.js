import React, { Component } from 'react';
import Websocket from 'react-websocket'; 

class App extends Component {

  onMessage = (message) => {
    console.log('got message', message);
  }

  render() {
    return (
      <div>
        Random content
        <Websocket url='ws://localhost:5000/event-stream'
            onMessage={this.onMessage}/>
      </div>
    );
  }
}

export default App;
