import { useState } from 'react'; // Removed the incorrect Link import

// If you're using react-router-dom, import Link like this
import { Link } from 'react-router-dom'; // Correct import for Link

function LoginCustomer() { // Capitalize component name for convention
  const [count, setCount] = useState(0);

  return (
    <>
      <header>
        <Link to="/employee">Medewerker</Link> {/* Added to prop for navigation */}
      </header>

      <div></div>

      <footer></footer>

      <div id="header"></div>

      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
    </>
  );
}

export default LoginCustomer; // Ensure you're using a default export
