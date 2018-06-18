import React, { Component } from 'react';
import Websocket from 'react-websocket'; 
import ReactMarkdown from 'react-markdown';
import {applyPatch} from 'fast-json-patch'

function TimelineEvent({event}) {
  return (
    <li key={event.id}><ReactMarkdown source={event.timelineMessage}/></li>
  );
}

function EventTimeline({events}) {
  return (<ul>{events.map(e => <TimelineEvent event={e}/>)}</ul>);
}

class AreaEditor extends Component {
  constructor(props) {
    super(props);
    this.state = {
      area: props.area,
      timelineMessage: ''
    }
  }

  onTimelineMessageChange = (event) => {
    const timelineMessage = event.target.value;
    this.setState({timelineMessage});
  }

  onDescriptionChange = (event) => {
    const description = event.target.value;
    this.setState(({area}) => {
      return {
        area: {
          ...area,
          description
        }
      };
    });
  }

  onSubmit = (e) => {
    e.preventDefault();
  }

  render() {
    const {area, timelineMessage} = this.state;
    return (
      <form onSubmit={this.onSubmit}>
        <label>
          Timeline Message:
          <textarea value={timelineMessage} onChange={this.onTimelineMessageChange}/>
        </label>
        <label>
          Description:
          <textarea value={area.description} onChange={this.onDescriptionChange}/>
        </label>
        <button type='submit'>Save</button>
      </form>
    );
  }
}

class App extends Component {

  constructor(props) {
    super(props);
    this.state = {
      eventStream: [],
      currentArea: {}
    }
  }

  onMessage = (messageBody) => {
    console.log(`got message${messageBody}`);
    const event = JSON.parse(messageBody);
    if (event.timelineMessage) {
      this.pushTimelineEvent(event);
    }
    if (event.areaPatchOperations) {
      this.setState(({currentArea}) => {
        const result = applyPatch(event.areaPatchOperations, currentArea);
        console.log("applied patch ", currentArea, event.areaPatchOperations, result);
        return {
          currentArea: result.newDocument
        };
      });
    }
  }

  pushTimelineEvent = (event) => {
    this.setState(({eventStream}) => {
      const newStream = [...eventStream, event].slice(-10);
      return {
        eventStream: newStream
      };
    });
  }

  render() {
    return (
      <div>
        Random content
        <Websocket url='ws://localhost:5000/event-stream'
            debug={true}
            onMessage={this.onMessage}/>
        <EventTimeline events={this.state.eventStream}/>
        <h2>Current Area</h2>
        <AreaEditor area={this.state.currentArea}/>
      </div>
    );
  }
}

export default App;
