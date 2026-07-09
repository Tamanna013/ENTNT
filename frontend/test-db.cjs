const sql = require('mssql');

const config = {
    server: '(localdb)\\mssqllocaldb',
    database: 'FleetMindDb',
    options: {
        trustedConnection: true,
        trustServerCertificate: true
    }
};

async function test() {
    try {
        await sql.connect(config);
        const result = await sql.query`SELECT TOP 1 Email FROM Users`;
        console.log("Success:", result.recordset);
    } catch (err) {
        console.error("Error:", err);
    } finally {
        process.exit();
    }
}
test();
